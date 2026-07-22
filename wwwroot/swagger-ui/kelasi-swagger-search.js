/**
 * KelasiNaBiso — Recherche Swagger UI (insensible à la casse).
 * Recherche le texte saisi dans : contrôleur, route, méthode, résumé, description.
 */
const KelasiSwaggerSearchPlugin = function () {
  function normalize(value) {
    return String(value || '').toLocaleLowerCase('fr-FR');
  }

  function containsPhrase(value, phrase) {
    return normalize(value).indexOf(phrase) !== -1;
  }

  function operationMatches(op, phrase) {
    return (
      containsPhrase(op.get('path'), phrase) ||
      containsPhrase(op.get('method'), phrase) ||
      containsPhrase(op.getIn(['operation', 'summary']), phrase) ||
      containsPhrase(op.getIn(['operation', 'description']), phrase) ||
      containsPhrase(op.getIn(['operation', 'operationId']), phrase)
    );
  }

  return {
    fn: {
      opsFilter: function (taggedOps, phrase) {
        if (!phrase || !String(phrase).trim()) {
          return taggedOps;
        }

        var q = normalize(String(phrase).trim());

        return taggedOps
          .map(function (tagObj, tag) {
            return tagObj.update('operations', function (operations) {
              return operations.filter(function (op) {
                return containsPhrase(tag, q) || operationMatches(op, q);
              });
            });
          })
          .filter(function (tagObj) {
            var ops = tagObj.get('operations');
            return ops && ops.size > 0;
          });
      }
    }
  };
};

(function () {
  function installPlugin(config) {
    config.plugins = (config.plugins || []).slice();
    config.plugins.push(KelasiSwaggerSearchPlugin);
    if (config.filter === undefined) {
      config.filter = true;
    }
    return config;
  }

  var originalBundle = window.SwaggerUIBundle;
  if (originalBundle) {
    window.SwaggerUIBundle = function (config) {
      return originalBundle(installPlugin(config || {}));
    };
    window.SwaggerUIBundle.prototype = originalBundle.prototype;
    Object.keys(originalBundle).forEach(function (key) {
      window.SwaggerUIBundle[key] = originalBundle[key];
    });
  }

  window.addEventListener('DOMContentLoaded', function () {
    var attempts = 0;
    var interval = setInterval(function () {
      attempts++;
      var input = document.querySelector('.swagger-ui .filter-container input[type="text"]');
      if (input) {
        input.placeholder = 'Rechercher un contrôleur, route ou endpoint…';
        input.setAttribute('aria-label', 'Rechercher un contrôleur ou endpoint');
        input.setAttribute('autocomplete', 'off');
        input.setAttribute('spellcheck', 'false');
        clearInterval(interval);
      }
      if (attempts > 100) {
        clearInterval(interval);
      }
    }, 100);
  });
})();
