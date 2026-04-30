using System;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace EnterpriseAuth.Saml
{
    /// Validates a SAML 2.0 Response from Okta / Ping Identity IdPs.
    /// Used in DocuSign and Tipalti SSO onboarding flows.
    public class SamlAssertionParser
    {
        private readonly X509Certificate2 _idpCertificate;

        public SamlAssertionParser(string certBase64)
        {
            var certBytes = Convert.FromBase64String(certBase64);
            _idpCertificate = new X509Certificate2(certBytes);
        }

        public SamlUser Parse(string samlResponseBase64)
        {
            var xml = new XmlDocument { PreserveWhitespace = true };
            xml.LoadXml(System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(samlResponseBase64)));

            ValidateSignature(xml);

            var ns = new XmlNamespaceManager(xml.NameTable);
            ns.AddNamespace("saml",
                "urn:oasis:names:tc:SAML:2.0:assertion");

            return new SamlUser
            {
                NameId = xml.SelectSingleNode(
                    "//saml:NameID", ns)?.InnerText,
                Email = GetAttribute(xml, ns, "email"),
                Role  = GetAttribute(xml, ns, "role"),
                TenantId = GetAttribute(xml, ns, "tenantId")
            };
        }

        private void ValidateSignature(XmlDocument doc)
        {
            var signed = new SignedXml(doc);
            var sigNode = doc.GetElementsByTagName("Signature")[0]
                as XmlElement;
            signed.LoadXml(sigNode);

            if (!signed.CheckSignature(_idpCertificate, true))
                throw new SecurityException("Invalid SAML signature");
        }

        private string GetAttribute(XmlDocument doc,
            XmlNamespaceManager ns, string name)
            => doc.SelectSingleNode(
                $"//saml:Attribute[@Name='{name}']/saml:AttributeValue",
                ns)?.InnerText;
    }

    public class SamlUser
    {
        public string NameId { get; set; }
        public string Email  { get; set; }
        public string Role   { get; set; }
        public string TenantId { get; set; }
    }
}
