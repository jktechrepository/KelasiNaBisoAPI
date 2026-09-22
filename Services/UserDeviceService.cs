using KelasiNaBiso.Data;
using KelasiNaBiso.Models;
using KelasiNaBisoAPI.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KelasiNaBisoAPI.Services
{
    /// <summary>
    /// Service pour la gestion des appareils utilisateurs (FCM tokens)
    /// </summary>
    public class UserDeviceService : IUserDeviceRepository
    {
        private readonly KelasiNaBisoDbContext _context;
        private readonly ILogger<UserDeviceService> _logger;

        public UserDeviceService(KelasiNaBisoDbContext context, ILogger<UserDeviceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDevice>> GetAllAsync()
        {
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .Where(ud => ud.Statut == true)
                .OrderByDescending(ud => ud.DateEnregistrement)
                .ToListAsync();
        }

        public async Task<UserDevice?> GetByIdAsync(int id)
        {
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .FirstOrDefaultAsync(ud => ud.IdUserDevice == id);
        }

        public async Task<UserDevice?> GetByFcmTokenAsync(string fcmToken)
        {
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .FirstOrDefaultAsync(ud => ud.FcmToken == fcmToken);
        }

        public async Task<IEnumerable<UserDevice>> GetByUtilisateurIdAsync(int idUtilisateur)
        {
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .Where(ud => ud.IdUtilisateur == idUtilisateur && ud.Statut == true)
                .OrderByDescending(ud => ud.DateDerniereUtilisation)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetActiveTokensByUtilisateurIdAsync(int idUtilisateur)
        {
            return await _context.UserDevices
                .Where(ud => ud.IdUtilisateur == idUtilisateur && ud.Statut == true)
                .Select(ud => ud.FcmToken)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetActiveTokensByRoleAsync(int idRole)
        {
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .Where(ud => ud.Utilisateur.IdRole == idRole && ud.Statut == true)
                .Select(ud => ud.FcmToken)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetActiveTokensByEcoleAsync(int idEcole)
        {
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .Where(ud => ud.Utilisateur.IdEcole == idEcole && ud.Statut == true)
                .Select(ud => ud.FcmToken)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetActiveTokensByClasseAsync(int idClasse)
        {
            // Pour les élèves d'une classe spécifique
            return await _context.UserDevices
                .Include(ud => ud.Utilisateur)
                .Where(ud => ud.Utilisateur.IdEcole != null && 
                           ud.Utilisateur.IdEcole == idClasse && 
                           ud.Statut == true)
                .Select(ud => ud.FcmToken)
                .ToListAsync();
        }

        public async Task<UserDevice> CreateAsync(UserDevice userDevice)
        {
            userDevice.DateEnregistrement = DateTime.Now;
            userDevice.Statut = true;

            _context.UserDevices.Add(userDevice);
            await _context.SaveChangesAsync();
            return userDevice;
        }

        /// <summary>
        /// Crée ou met à jour un device pour un utilisateur
        /// ✅ CORRIGÉ: Recherche par UserId + DeviceType pour permettre plusieurs devices par utilisateur
        /// ✅ CORRIGÉ: Validation des données pour éviter les valeurs "string" par défaut
        /// </summary>
        public async Task<UserDevice> CreateOrUpdateAsync(int idUtilisateur, string fcmToken, string? deviceType = null, string? deviceModel = null, string? osVersion = null, string? appVersion = null)
        {
            // 🚨 VALIDATION: Rejeter les valeurs par défaut "string"
            if (string.IsNullOrWhiteSpace(fcmToken) || fcmToken == "string" || fcmToken == "null")
            {
                throw new ArgumentException("FCM Token invalide", nameof(fcmToken));
            }

            if (string.IsNullOrWhiteSpace(deviceType) || deviceType == "string" || deviceType == "null")
            {
                throw new ArgumentException("Device Type invalide", nameof(deviceType));
            }

            // ✅ CORRIGÉ: Chercher par UserId + DeviceType (au lieu de FCM Token uniquement)
            // Cela permet à un utilisateur d'avoir plusieurs devices (ex: Android + iOS)
            var existingDevice = await _context.UserDevices
                .FirstOrDefaultAsync(ud => ud.IdUtilisateur == idUtilisateur && ud.DeviceType == deviceType);

            if (existingDevice != null)
            {
                // Mettre à jour le device existant pour ce type
                existingDevice.FcmToken = fcmToken; // Le token peut changer
                existingDevice.DeviceModel = deviceModel ?? existingDevice.DeviceModel;
                existingDevice.OsVersion = osVersion ?? existingDevice.OsVersion;
                if (!string.IsNullOrWhiteSpace(appVersion))
                    existingDevice.AppVersion = appVersion.Trim();
                existingDevice.DateDerniereUtilisation = DateTime.Now;
                existingDevice.Statut = true;

                await _context.SaveChangesAsync();
                return existingDevice;
            }
            else
            {
                // Créer un nouveau device pour ce type
                var newDevice = new UserDevice
                {
                    IdUtilisateur = idUtilisateur,
                    FcmToken = fcmToken,
                    DeviceType = deviceType,
                    DeviceModel = deviceModel ?? "Unknown",
                    OsVersion = osVersion ?? "Unknown",
                    AppVersion = string.IsNullOrWhiteSpace(appVersion) ? null : appVersion.Trim(),
                    DateEnregistrement = DateTime.Now,
                    DateDerniereUtilisation = DateTime.Now,
                    Statut = true
                };

                _context.UserDevices.Add(newDevice);
                await _context.SaveChangesAsync();
                return newDevice;
            }
        }

        public async Task<UserDevice?> UpdateAsync(UserDevice userDevice)
        {
            var existingDevice = await _context.UserDevices.FindAsync(userDevice.IdUserDevice);
            if (existingDevice == null)
                return null;

            _context.Entry(existingDevice).CurrentValues.SetValues(userDevice);
            await _context.SaveChangesAsync();
            return existingDevice;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var device = await _context.UserDevices.FindAsync(id);
            if (device == null)
                return false;

            _context.UserDevices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByFcmTokenAsync(string fcmToken)
        {
            var device = await _context.UserDevices
                .FirstOrDefaultAsync(ud => ud.FcmToken == fcmToken);
            
            if (device == null)
                return false;

            _context.UserDevices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.UserDevices.AnyAsync(ud => ud.IdUserDevice == id);
        }

        public async Task<bool> ExistsByFcmTokenAsync(string fcmToken)
        {
            return await _context.UserDevices.AnyAsync(ud => ud.FcmToken == fcmToken);
        }
    }
}
