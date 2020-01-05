using System;
using System.Collections.Generic;
using System.Diagnostics;

using NLog;

using Shadowsocks.Std.Encryption.Exception;
using Shadowsocks.Std.Util;

namespace Shadowsocks.Std.Encryption.AEAD
{
    public class AEADSodiumEncryptor
        : AEADEncryptor, IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private const int CIPHER_CHACHA20IETFPOLY1305 = 1;
        private const int CIPHER_XCHACHA20IETFPOLY1305 = 2;
        private const int CIPHER_AES256GCM = 3;

        private readonly byte[] _sodiumEncSubkey;
        private readonly byte[] _sodiumDecSubkey;

        public AEADSodiumEncryptor(string method, string password)
            : base(method, password)
        {
            _sodiumEncSubkey = new byte[keyLen];
            _sodiumDecSubkey = new byte[keyLen];
        }

        private static readonly Dictionary<string, EncryptorInfo> _ciphers = new Dictionary<string, EncryptorInfo>
        {
            {"chacha20-ietf-poly1305", new EncryptorInfo(32, 32, 12, 16, CIPHER_CHACHA20IETFPOLY1305)},
            {"xchacha20-ietf-poly1305", new EncryptorInfo(32, 32, 24, 16, CIPHER_XCHACHA20IETFPOLY1305)},
            {"aes-256-gcm", new EncryptorInfo(32, 32, 12, 16, CIPHER_AES256GCM)},
        };

        public static List<string> SupportedCiphers()
        {
            return new List<string>(_ciphers.Keys);
        }

        protected override Dictionary<string, EncryptorInfo> getCiphers()
        {
            return _ciphers;
        }

        public override void InitCipher(byte[] salt, bool isEncrypt, bool isUdp)
        {
            base.InitCipher(salt, isEncrypt, isUdp);
            DeriveSessionKey(isEncrypt ? _encryptSalt : _decryptSalt, _Masterkey,
                isEncrypt ? _sodiumEncSubkey : _sodiumDecSubkey);
        }

        public override void cipherEncrypt(byte[] plaintext, uint plen, byte[] ciphertext, ref uint clen)
        {
            Debug.Assert(_sodiumEncSubkey != null);
            // buf: all plaintext
            // outbuf: ciphertext + tag
            int ret;
            ulong encClen = 0;
            _logger.Dump("_encNonce before enc", _encNonce, nonceLen);
            _logger.Dump("_sodiumEncSubkey", _sodiumEncSubkey, keyLen);
            _logger.Dump("before cipherEncrypt: plain", plaintext, (int)plen);
            switch (_cipher)
            {
                case CIPHER_CHACHA20IETFPOLY1305:
                    ret = Sodium.crypto_aead_chacha20poly1305_ietf_encrypt(ciphertext, ref encClen,
                        plaintext, (ulong)plen,
                        null, 0,
                        null, _encNonce,
                        _sodiumEncSubkey);
                    break;

                case CIPHER_XCHACHA20IETFPOLY1305:
                    ret = Sodium.crypto_aead_xchacha20poly1305_ietf_encrypt(ciphertext, ref encClen,
                        plaintext, (ulong)plen,
                        null, 0,
                        null, _encNonce,
                        _sodiumEncSubkey);
                    break;

                case CIPHER_AES256GCM:
                    ret = Sodium.crypto_aead_aes256gcm_encrypt(ciphertext, ref encClen,
                        plaintext, (ulong)plen,
                        null, 0,
                        null, _encNonce,
                        _sodiumEncSubkey);
                    break;

                default:
                    throw new System.Exception("not implemented");
            }
            if (ret != 0) throw new CryptoErrorException(String.Format("ret is {0}", ret));
            _logger.Dump("after cipherEncrypt: cipher", ciphertext, (int)encClen);
            clen = (uint)encClen;
        }

        public override void cipherDecrypt(byte[] ciphertext, uint clen, byte[] plaintext, ref uint plen)
        {
            Debug.Assert(_sodiumDecSubkey != null);
            // buf: ciphertext + tag
            // outbuf: plaintext
            int ret;
            ulong decPlen = 0;
            _logger.Dump("_decNonce before dec", _decNonce, nonceLen);
            _logger.Dump("_sodiumDecSubkey", _sodiumDecSubkey, keyLen);
            _logger.Dump("before cipherDecrypt: cipher", ciphertext, (int)clen);
            switch (_cipher)
            {
                case CIPHER_CHACHA20IETFPOLY1305:
                    ret = Sodium.crypto_aead_chacha20poly1305_ietf_decrypt(plaintext, ref decPlen,
                        null,
                        ciphertext, (ulong)clen,
                        null, 0,
                        _decNonce, _sodiumDecSubkey);
                    break;

                case CIPHER_XCHACHA20IETFPOLY1305:
                    ret = Sodium.crypto_aead_xchacha20poly1305_ietf_decrypt(plaintext, ref decPlen,
                        null,
                        ciphertext, (ulong)clen,
                        null, 0,
                        _decNonce, _sodiumDecSubkey);
                    break;

                case CIPHER_AES256GCM:
                    ret = Sodium.crypto_aead_aes256gcm_decrypt(plaintext, ref decPlen,
                        null,
                        ciphertext, (ulong)clen,
                        null, 0,
                        _decNonce, _sodiumDecSubkey);
                    break;

                default:
                    throw new System.Exception("not implemented");
            }

            if (ret != 0) throw new CryptoErrorException(String.Format("ret is {0}", ret));
            _logger.Dump("after cipherDecrypt: plain", plaintext, (int)decPlen);
            plen = (uint)decPlen;
        }

        public override void Dispose()
        {
        }
    }
}