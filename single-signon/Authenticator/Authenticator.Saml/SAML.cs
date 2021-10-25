// https://github.com/jitbit/AspNetSaml/

using System;
using System.Web;
using System.IO;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.IO.Compression;
using System.Text;

[assembly: CLSCompliant(false)]
namespace Authenticator.Saml
{
    public class AssertionResponse : IDisposable
    {
        protected XmlDocument xmlDocument;
        protected readonly X509Certificate2 x509Certificate;
        protected XmlNamespaceManager xmlNameSpaceManager;

        public string Xml => xmlDocument.OuterXml;

        public AssertionResponse(string certificateString, string responseString) : this(StringToByteArray(string.IsNullOrEmpty(certificateString) ? string.Empty : certificateString), responseString) { }

        public AssertionResponse(byte[] certificateBytes, string responseString) : this(certificateBytes)
        {
            LoadXmlFromBase64(responseString);
        }

        public AssertionResponse(string certificateString) : this(StringToByteArray(string.IsNullOrEmpty(certificateString) ? string.Empty : certificateString)) { }

        public AssertionResponse(byte[] certificateBytes)
        {
            x509Certificate = new X509Certificate2(certificateBytes);
        }

        public void LoadXml(string xml)
        {
            xmlDocument = new XmlDocument
            {
                PreserveWhitespace = true,
                XmlResolver = null
            };
            xmlDocument.LoadXml(xml);
            xmlNameSpaceManager = GetNamespaceManager();
        }

        public void LoadXmlFromBase64(string response)
        {
            var encoding = new UTF8Encoding();

            var decodedResponse = Convert.FromBase64String(response);

            LoadXml(encoding.GetString(decodedResponse));
        }

        public bool IsValid()
        {
            var signedXml = new SignedXml(xmlDocument);

            var nodeList = xmlDocument.SelectNodes("//ds:Signature", xmlNameSpaceManager);
            if (nodeList is { Count: 0 })
            {
                return false;
            }

            if (nodeList != null)
            {
                signedXml.LoadXml((XmlElement)nodeList[0]);
            }
            return ValidateSignatureReference(signedXml) && signedXml.CheckSignature(x509Certificate, true) && !IsExpired();
        }

        public string GetNameId()
        {
            var node = xmlDocument.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:Subject/saml:NameID", xmlNameSpaceManager);
            return node == null ? string.Empty : node.InnerText;
        }

        public virtual string GetUpn()
        {
            return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn");
        }

        public virtual string GetFirstName()
        {
            return GetCustomAttribute("first_name")
                ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")
                ?? GetCustomAttribute("User.FirstName")
                ?? GetCustomAttribute("givenName");
        }

        public virtual string GetLastName()
        {
            return GetCustomAttribute("last_name")
                ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")
                ?? GetCustomAttribute("User.LastName")
                ?? GetCustomAttribute("sn");
        }

        public virtual string GetEmail()
        {
            return GetCustomAttribute("User.email")
                ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                ?? GetCustomAttribute("mail");
        }

        public virtual string GetPhone()
        {
            return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone")
                ?? GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/telephonenumber");
        }

        public virtual string GetCompany()
        {
            return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/companyname")
                ?? GetCustomAttribute("organization")
                ?? GetCustomAttribute("User.CompanyName");
        }

        public virtual string GetDepartment()
        {
            return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/department")
                ?? GetCustomAttribute("department");
        }

        public virtual string GetLocation()
        {
            return GetCustomAttribute("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/location")
                ?? GetCustomAttribute("physicalDeliveryOfficeName");
        }

        public string GetCustomAttribute(string attribute)
        {
            var node = xmlDocument.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:AttributeStatement/saml:Attribute[@Name='" + attribute + "']/saml:AttributeValue", xmlNameSpaceManager);
            return node?.InnerText;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                x509Certificate.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private XmlNamespaceManager GetNamespaceManager()
        {
            var manager = new XmlNamespaceManager(xmlDocument.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            return manager;
        }

        private bool IsExpired()
        {
            var expirationDate = DateTime.MaxValue;
            var node = xmlDocument.SelectSingleNode("/samlp:Response/saml:Assertion[1]/saml:Subject/saml:SubjectConfirmation/saml:SubjectConfirmationData", xmlNameSpaceManager);
            if (node?.Attributes?["NotOnOrAfter"] != null)
            {
                _ = DateTime.TryParse(node.Attributes["NotOnOrAfter"].Value, out expirationDate);
            }

            return DateTime.UtcNow > expirationDate.ToUniversalTime();
        }

        private bool ValidateSignatureReference(SignedXml signedXml)
        {
            if (signedXml.SignedInfo.References.Count != 1)
            {
                return false;
            }

            var reference = (Reference)signedXml.SignedInfo.References[0];
            if (reference != null)
            {
                var id = reference.Uri[1..];

                var idElement = signedXml.GetIdElement(xmlDocument, id);
                if (idElement == xmlDocument.DocumentElement)
                {
                    return true;
                }

                var assertionNode = xmlDocument.SelectSingleNode("/samlp:Response/saml:Assertion", xmlNameSpaceManager) as XmlElement;
                if (assertionNode != idElement)
                {
                    return false;
                }
            }

            return true;
        }

        private static byte[] StringToByteArray(string value)
        {
            var bytes = new byte[value.Length];
            for (var i = 0; i < value.Length; i++)
            {
                bytes[i] = (byte)value[i];
            }
            return bytes;
        }
    }

    public class AuthenticationRequest
    {
        private readonly string issueInstant;
        private readonly string issuer;
        private readonly string assertionConsumerServiceUrl;

        public enum AuthenticationRequestFormat
        {
            None,
            Base64 = 1
        }

        public AuthenticationRequest(string authenticationIssuer, string authenticationAssertionConsumerServiceUrl)
        {
            Id = "_" + Guid.NewGuid().ToString();
            issueInstant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            issuer = authenticationIssuer;
            assertionConsumerServiceUrl = authenticationAssertionConsumerServiceUrl;
        }

        public string GetRedirectUrl(string samlEndpoint, string relayState = null)
        {
            if (string.IsNullOrEmpty(samlEndpoint))
            {
                throw new ArgumentNullException(nameof(samlEndpoint));
            }

            var queryStringSeparator = samlEndpoint.Contains("?", StringComparison.OrdinalIgnoreCase) ? "&" : "?";

            var url = $"{samlEndpoint}{queryStringSeparator}SAMLRequest={HttpUtility.UrlEncode(GetRequest(AuthenticationRequestFormat.Base64))}";
            if (!string.IsNullOrEmpty(relayState))
            {
                url += "&RelayState=" + HttpUtility.UrlEncode(relayState);
            }
            return url;
        }

        public string GetRequest(AuthenticationRequestFormat format)
        {
            using var stringWriter = new StringWriter();
            var xmlWriterSettings = new XmlWriterSettings { OmitXmlDeclaration = true };

            using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
            {
                xmlWriter.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                xmlWriter.WriteAttributeString("ID", Id);
                xmlWriter.WriteAttributeString("Version", "2.0");
                xmlWriter.WriteAttributeString("IssueInstant", issueInstant);
                xmlWriter.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                xmlWriter.WriteAttributeString("AssertionConsumerServiceURL", assertionConsumerServiceUrl);

                xmlWriter.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                xmlWriter.WriteString(issuer);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                xmlWriter.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
                xmlWriter.WriteAttributeString("AllowCreate", "true");
                xmlWriter.WriteEndElement();
            }

            if (format == AuthenticationRequestFormat.Base64)
            {
                var memoryStream = new MemoryStream();

                var writer = new StreamWriter(new DeflateStream(memoryStream, CompressionMode.Compress, true), new UTF8Encoding(false));
                writer.Write(stringWriter.ToString());
                writer.Close();

                return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length, Base64FormattingOptions.None);
            }
            return null;
        }

        public string Id;
    }
}