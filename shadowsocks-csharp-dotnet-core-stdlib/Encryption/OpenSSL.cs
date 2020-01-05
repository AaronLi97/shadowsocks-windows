using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using NLog;

using Shadowsocks.Std.Encryption.Exception;
using Shadowsocks.Std.Util;

using static Shadowsocks.Std.Encryption.DelegateOpenSSL;

namespace Shadowsocks.Std.Encryption
{
    // XXX: only for OpenSSL 1.1.0 and higher
    public static class OpenSSL
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public const int OPENSSL_ENCRYPT = 1;
        public const int OPENSSL_DECRYPT = 0;

        public const int EVP_CTRL_AEAD_SET_IVLEN = 0x9;
        public const int EVP_CTRL_AEAD_GET_TAG = 0x10;
        public const int EVP_CTRL_AEAD_SET_TAG = 0x11;

        static OpenSSL()
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

        public static IntPtr GetCipherInfo(string cipherName)
        {
            var name = Encoding.ASCII.GetBytes(cipherName);
            Array.Resize(ref name, name.Length + 1);
            return EVP_get_cipherbyname(name);
        }

        /// <summary>
        /// Need init cipher context after EVP_CipherFinal_ex to reuse context
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="cipherType"></param>
        /// <param name="nonce"></param>
        public static void SetCtxNonce(IntPtr ctx, byte[] nonce, bool isEncrypt)
        {
            var ret = EVP_CipherInit_ex(ctx, IntPtr.Zero,
                IntPtr.Zero, null,
                nonce,
                isEncrypt ? OPENSSL_ENCRYPT : OPENSSL_DECRYPT);
            if (ret != 1) throw new System.Exception("openssl: fail to set AEAD nonce");
        }

        public static void AEADGetTag(IntPtr ctx, byte[] tagbuf, int taglen)
        {
            IntPtr tagBufIntPtr = IntPtr.Zero;
            try
            {
                tagBufIntPtr = Marshal.AllocHGlobal(taglen);
                var ret = EVP_CIPHER_CTX_ctrl(ctx,
                    EVP_CTRL_AEAD_GET_TAG, taglen, tagBufIntPtr);
                if (ret != 1) throw new CryptoErrorException("openssl: fail to get AEAD tag");
                // take tag from unmanaged memory
                Marshal.Copy(tagBufIntPtr, tagbuf, 0, taglen);
            }
            finally
            {
                if (tagBufIntPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tagBufIntPtr);
                }
            }
        }

        public static void AEADSetTag(IntPtr ctx, byte[] tagbuf, int taglen)
        {
            IntPtr tagBufIntPtr = IntPtr.Zero;
            try
            {
                // allocate unmanaged memory for tag
                tagBufIntPtr = Marshal.AllocHGlobal(taglen);

                // copy tag to unmanaged memory
                Marshal.Copy(tagbuf, 0, tagBufIntPtr, taglen);

                var ret = EVP_CIPHER_CTX_ctrl(ctx, EVP_CTRL_AEAD_SET_TAG, taglen, tagBufIntPtr);

                if (ret != 1) throw new CryptoErrorException("openssl: fail to set AEAD tag");
            }
            finally
            {
                if (tagBufIntPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tagBufIntPtr);
                }
            }
        }

        public static EVP_CIPHER_CTX_new EVP_CIPHER_CTX_new;

        public static EVP_CIPHER_CTX_free EVP_CIPHER_CTX_free;

        public static EVP_CIPHER_CTX_reset EVP_CIPHER_CTX_reset;

        public static EVP_CipherInit_ex EVP_CipherInit_ex;

        public static EVP_CipherUpdate EVP_CipherUpdate;

        public static EVP_CipherFinal_ex EVP_CipherFinal_ex;

        public static EVP_CIPHER_CTX_set_padding EVP_CIPHER_CTX_set_padding;

        public static EVP_CIPHER_CTX_set_key_length EVP_CIPHER_CTX_set_key_length;

        public static EVP_CIPHER_CTX_ctrl EVP_CIPHER_CTX_ctrl;

        /// <summary>
        /// simulate NUL-terminated string
        /// </summary>
        public static EVP_get_cipherbyname EVP_get_cipherbyname;
    }
}