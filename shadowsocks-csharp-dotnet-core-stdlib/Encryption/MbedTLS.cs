using System.IO;

using NLog;

using Shadowsocks.Std.Util;

using static Shadowsocks.Std.Encryption.DelegateMbedTLS;

namespace Shadowsocks.Std.Encryption
{
    public static class MbedTLS
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public const int MBEDTLS_ENCRYPT = 1;
        public const int MBEDTLS_DECRYPT = 0;

        static MbedTLS()
        {
            try
            {
                Utils.GetAndUncompressLib(Utils.libsscrypto);
            }
            catch (IOException)
            {
            }
            catch (System.Exception e)
            {
                _logger.LogUsefulException(e);
            }
        }

        public static byte[] MD5(byte[] input)
        {
            byte[] output = new byte[16];
            if (md5_ret(input, (uint)input.Length, output) != 0)
                throw new System.Exception("mbedtls: MD5 failure");
            return output;
        }

        public static md5_ret md5_ret;

        /// <summary>
        /// Get cipher ctx size for unmanaged memory allocation
        /// </summary>
        /// <returns></returns>
        public static cipher_get_size_ex cipher_get_size_ex;

        #region Cipher layer wrappers

        public static cipher_info_from_string cipher_info_from_string;

        public static cipher_init cipher_init;

        public static cipher_setup cipher_setup;

        // XXX: Check operation before using it
        public static cipher_setkey cipher_setkey;

        public static cipher_set_iv cipher_set_iv;

        public static cipher_reset cipher_reset;

        public static cipher_update cipher_update;

        public static cipher_free cipher_free;

        public static cipher_auth_encrypt cipher_auth_encrypt;

        public static cipher_auth_decrypt cipher_auth_decrypt;

        public static hkdf hkdf;

        #endregion Cipher layer wrappers
    }
}