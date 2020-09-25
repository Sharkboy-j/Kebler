using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Kebler.Services
{
    public static class SecureStorage
    {
        /// <summary>
        /// Yeah! YEah! Fucking usefull!! I know. STFU
        /// </summary>
        #region sdsdsdsd
        private const string Entropy = "#X$DRCFT&V *YGBUHIOKMHN(UGBYFVTCRRDTFV YG(UH)IJOK_MJ)IN(HвUBGYTV^R%C%CRv6btynumi0nju9yb8TV&C&TYGB";



        public static string EncryptString(string data)
        {
            var input = SecureString(data);

            var encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(UnSecureString(input)),
                Encoding.Unicode.GetBytes(Entropy),
                DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedData);
        }

        //public static SecureString DecryptString(string encryptedData)
        //{
        //    try
        //    {
        //        var decryptedData = ProtectedData.Unprotect(
        //            Convert.FromBase64String(encryptedData),
        //            Encoding.Unicode.GetBytes(Entropy),
        //            DataProtectionScope.CurrentUser);
        //        return SecureString(Encoding.Unicode.GetString(decryptedData));
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        public static string DecryptStringAndUnSecure(string encryptedData)
        {
            try
            {
                var decryptedData = ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    Encoding.Unicode.GetBytes(Entropy),
                    DataProtectionScope.CurrentUser);
                return UnSecureString(SecureString(Encoding.Unicode.GetString(decryptedData)));
            }
            catch
            {
                return null;
            }
        }

        private static string UnSecureString(SecureString input)
        {
            string returnValue;
            var ptr = Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }

            return returnValue;
        }

        private static SecureString SecureString(string input)
        {
            var secure = new SecureString();
            foreach (var c in input)
            {
                secure.AppendChar(c);
            }

            secure.MakeReadOnly();
            return secure;
        }

        #endregion
    }
}
