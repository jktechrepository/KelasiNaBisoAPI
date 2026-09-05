-- Ops inspect : transaction Moko citée dans le log Flutter (PayIn Mpesa soft Error)
-- Exécuter sur la base métier (ex. knb_db / dev-knb_db), PAS information_schema.

SELECT DATABASE() AS current_db;

SELECT
  t.IdTransactionMoko,
  t.Reference,
  t.IdPaiement,
  t.IdEcole,
  t.Action,
  t.Amount,
  t.AmountNet,
  t.Devise,
  t.Method,
  t.Status,
  t.StatusDescription,
  t.GatewayTransactionId,
  t.DateCreation,
  t.DateModification,
  LEFT(t.RawResponse, 800) AS RawResponse_preview,
  LEFT(t.RawCallback, 800) AS RawCallback_preview
FROM TransactionsMoko t
WHERE t.Reference = 'MOKO_20260904141445_8948';

SELECT
  p.IdPaiement,
  p.IdEleve,
  p.IdFrais,
  p.Montant,
  p.MontantNet,
  p.MontantCollecte,
  p.Devise,
  p.StatutPaiement,
  p.ModePaiement,
  p.OperateurMobileMoney,
  p.ReferenceTransaction,
  p.ReferencePaiemenet,
  p.DatePaiement,
  p.DateCreation
FROM Paiements p
WHERE p.IdPaiement = 803
   OR p.ReferencePaiemenet = 'MOKO_20260904141445_8948'
   OR p.ReferenceTransaction = 'MOKO_20260904141445_8948';

-- Interprétation :
-- RawCallback avec Status/Error sans resultCodeError + Status local error
--   => ancien bug callback soft (corrigé : soft → pending)
-- RawResponse check avec Status Error + Status local error
--   => ancien bug CheckStatus soft (corrigé : soft → pending)
-- Devise CDF alors que le front a envoyé USD
--   => devise école imposée par l'orchestrateur (attendu)
