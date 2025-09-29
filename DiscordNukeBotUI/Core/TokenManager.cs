using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage; // 1. 引入必要的命名空間

namespace DiscordNukeBot.Core
{
    public static class TokenManager
    {
        private const string TokenFileName = "bot_token.dat"; // 只保留檔案名稱
        // Salt 用於增加 DPAPI 加密的複雜度，保持不變
        private static readonly byte[] Salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        /// <summary>
        /// 取得應用程式本機儲存區中 Token 檔案的完整路徑。
        /// </summary>
        /// <returns>一個絕對且合法的檔案路徑。</returns>
        private static string GetTokenFilePath()
        {
            // 2. 取得應用程式的本機資料夾 (例如 C:\Users\...\AppData\Local\Packages\...\LocalState)
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            // 組合出設定檔的完整路徑
            return Path.Combine(localFolder.Path, TokenFileName);
        }

        public static string GetToken()
        {
            string filePath = GetTokenFilePath(); // 3. 使用新的方法取得路徑

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
                // 4. 針對不同的錯誤類型提供更精確的日誌
                catch (CryptographicException ex)
                {
                    AppLogger.Error($"Failed to decrypt token: {ex.Message}. The token file might be corrupted or was created on a different machine/user account.");
                    // 解密失敗時，最好刪除無效的檔案，強制使用者重新輸入
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

            string filePath = GetTokenFilePath(); // 3. 使用新的方法取得路徑

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
            string filePath = GetTokenFilePath(); // 3. 使用新的方法取得路徑

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
            // 使用 DPAPI 保護資料，範圍設定為目前使用者
            byte[] encryptedBytes = ProtectedData.Protect(plainTextBytes, Salt, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            // 從目前使用者的範圍解密資料
            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, Salt, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
