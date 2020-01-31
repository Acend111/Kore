using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{

    public static class QuestSystemSerializationHelper
    {

        private const string strEncrypt = "*#4$%^.++q~!cfr0(_!#$@$!&#&#*&@(7cy9rn8r265&$@&*E^Tw4ndMel2cr9o3r6329";

        public static CryptoStream GetCryptoStream(FileStream stream, CryptoStreamMode mode)
        {
            byte[] dv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            var byKey = Encoding.UTF8.GetBytes(strEncrypt.Substring(0, 8));
            var des = new DESCryptoServiceProvider();
            if (mode == CryptoStreamMode.Write)
                return new CryptoStream(stream, des.CreateEncryptor(byKey, dv), mode);
            else
                return new CryptoStream(stream, des.CreateDecryptor(byKey, dv), mode);
        }
    }

}