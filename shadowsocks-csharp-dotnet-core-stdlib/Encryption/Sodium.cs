using System.IO;

using NLog;

using Shadowsocks.Std.Util;

using static Shadowsocks.Std.Encryption.DelegateSodium;

namespace Shadowsocks.Std.Encryption
{
    public static class Sodium
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static bool _initialized = false;
        private static readonly object _initLock = new object();

        public static bool AES256GCMAvailable { get; private set; } = false;

        static Sodium()
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

            lock (_initLock)
            {
                if (!_initialized)
                {
                    if (sodium_init() == -1)
                    {
                        throw new System.Exception("Failed to initialize sodium");
                    }
                    else /* 1 means already initialized; 0 means success */
                    {
                        _initialized = true;
                    }

                    AES256GCMAvailable = crypto_aead_aes256gcm_is_available() == 1;
                    _logger.Debug($"sodium: AES256GCMAvailable is {AES256GCMAvailable}");
                }
            }
        }

        public static Sodium_init sodium_init;

        public static Crypto_aead_aes256gcm_is_available crypto_aead_aes256gcm_is_available;

        #region AEAD

        public static Sodium_increment sodium_increment;

        public static Crypto_aead_chacha20poly1305_ietf_encrypt crypto_aead_chacha20poly1305_ietf_encrypt;

        public static Crypto_aead_chacha20poly1305_ietf_decrypt crypto_aead_chacha20poly1305_ietf_decrypt;

        public static Crypto_aead_xchacha20poly1305_ietf_encrypt crypto_aead_xchacha20poly1305_ietf_encrypt;

        public static Crypto_aead_xchacha20poly1305_ietf_decrypt crypto_aead_xchacha20poly1305_ietf_decrypt;

        public static Crypto_aead_aes256gcm_encrypt crypto_aead_aes256gcm_encrypt;

        public static Crypto_aead_aes256gcm_decrypt crypto_aead_aes256gcm_decrypt;

        #endregion AEAD

        #region Stream

        public static Crypto_stream_salsa20_xor_ic crypto_stream_salsa20_xor_ic;

        public static Crypto_stream_chacha20_xor_ic crypto_stream_chacha20_xor_ic;

        public static Crypto_stream_chacha20_ietf_xor_ic crypto_stream_chacha20_ietf_xor_ic;

        #endregion Stream
    }
}