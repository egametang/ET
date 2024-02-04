using UnityEngine.Networking;

namespace ET
{
    public class AcceptAllCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}