using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage; // 1. �ޤJ���n���R�W�Ŷ�

namespace DiscordNukeBot.Core
{
    public static class TokenManager
    {
        private const string TokenFileName = "bot_token.dat"; // �u�O�d�ɮצW��
        // Salt �Ω�W�[ DPAPI �[�K�������סA�O������
        private static readonly byte[] Salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        /// <summary>
        /// ���o���ε{�������x�s�Ϥ� Token �ɮת�������|�C
        /// </summary>
        /// <returns>�@�ӵ���B�X�k���ɮ׸��|�C</returns>
        private static string GetTokenFilePath()
        {
            // 2. ���o���ε{����������Ƨ� (�Ҧp C:\Users\...\AppData\Local\Packages\...\LocalState)
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            // �զX�X�]�w�ɪ�������|
            return Path.Combine(localFolder.Path, TokenFileName);
        }

        public static string GetToken()
        {
            string filePath = GetTokenFilePath(); // 3. �ϥηs����k���o���|

            if (File.Exists(filePath))
            {
                try
                {
                    string encryptedToken = File.ReadAllText(filePath);
                    if (string.IsNullOrWhiteSpace(encryptedToken))
                    {
                        AppLogger.Warning("Token file is empty.");
                        return null;
                    }

                    string token = Decrypt(encryptedToken);
                    AppLogger.Success("Successfully decrypted and read token.");
                    return token;
                }
                // 4. �w�藍�P�����~�������ѧ��T����x
                catch (CryptographicException ex)
                {
                    AppLogger.Error($"Failed to decrypt token: {ex.Message}. The token file might be corrupted or was created on a different machine/user account.");
                    // �ѱK���ѮɡA�̦n�R���L�Ī��ɮסA�j��ϥΪ̭��s��J
                    DeleteTokenFile();
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"Failed to read token file: {ex.Message}");
                }
            }

            AppLogger.Info("Saved bot token not found. User needs to provide a new token.");
            return null;
        }

        public static void SaveToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                AppLogger.Warning("Attempted to save an empty token. Operation cancelled.");
                return;
            }

            string filePath = GetTokenFilePath(); // 3. �ϥηs����k���o���|

            try
            {
                string encryptedToken = Encrypt(token);
                File.WriteAllText(filePath, encryptedToken);
                AppLogger.Success($"Token has been encrypted and saved successfully.");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Failed to save token: {ex.Message}");
            }
        }

        public static void DeleteTokenFile()
        {
            string filePath = GetTokenFilePath(); // 3. �ϥηs����k���o���|

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    AppLogger.Info("Token file deleted successfully.");
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"Failed to delete token file: {ex.Message}");
                }
            }
        }

        private static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            // �ϥ� DPAPI �O�@��ơA�d��]�w���ثe�ϥΪ�
            byte[] encryptedBytes = ProtectedData.Protect(plainTextBytes, Salt, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            // �q�ثe�ϥΪ̪��d��ѱK���
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, Salt, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
