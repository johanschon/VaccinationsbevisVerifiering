﻿using System;
using System.Collections.Generic;
using VaccinbevisVerifiering.Services.CWT.Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using VaccinbevisVerifiering.Resources;

/**
* A HCert verifier class.
*
* @author Henrik Bengtsson (henrik@sondaica.se)
* @author Martin Lindström (martin@idsec.se)
* @author Henric Norlander (extern.henric.norlander@digg.se)
*/
namespace VaccinbevisVerifiering.Services.CWT
{
    public class DGCVerifier
    {
        private readonly ICertificateProvider certificateProvider;

        public DGCVerifier(ICertificateProvider certificateProvider)
        {
            this.certificateProvider = certificateProvider;
        }

        /**
         * Verifies the supplied signed DGC. If verification is successful the method returns the contained HCERT
         * (eu_hcert_v1) in its binary representation.
         * 
         * @throws SignatureException
         *           if signature validation fails
         * @throws CertificateExpiredException
         *           if the DGC has expired
         */
        public byte[] Verify(byte[] signedDGC, SignedDGC vacProof)
        {
            CoseSign1_Object obj = CoseSign1_Object.Decode(signedDGC);

            byte[] kid = obj.GetKeyIdentifier();
            string country = obj.GetCwt().GetIssuer();

            vacProof.IssuingCountry = country;

            if (kid == null && country == null)
            {
                throw new Exception(AppResources.InvalidSigningCertificate);
            }

            List<AsymmetricKeyParameter> certs = certificateProvider.GetCertificates(country, kid);

            // No validation only debug
            if (Xamarin.Essentials.Preferences.Get("NoVerificationMode", false))
            {
                CWT cwt2 = obj.GetCwt();
                DateTime? expiration = cwt2.GetExpiration();
                if (expiration.HasValue)
                {
                    vacProof.ExpirationDate = expiration.Value;
                    if (DateTime.UtcNow.CompareTo(expiration) >= 0)
                    {
                        vacProof.Message=string.Format("Expired {0}", expiration.Value);
                    }
                }
                else
                {
                    vacProof.Message="No expiration time";
                }
                DateTime? issuedAt = cwt2.GetIssuedAt();
                if (issuedAt.HasValue)
                {
                    vacProof.IssuedDate = issuedAt.Value;
                }
                if (certs.Count <= 0)
                {
                    string kidString = System.Text.Encoding.UTF8.GetString(kid);
                    vacProof.Message=string.Format("No cert found {0}-{1}",country, kidString);
                }
                return cwt2.GetDgcV1();
            }

            foreach (AsymmetricKeyParameter cert in certs)
            {
                //Console.WriteLine("Attempting HCERT signature verification using certificate");// '{0}'", cert.Subject);//getSubjectX500Principal().getName()) ;

                try {
                    byte[] key = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(cert).GetEncoded();
                    obj.VerifySignature(key);
                    string keyString = System.Text.Encoding.UTF8.GetString(key);
                    //Console.WriteLine("HCERT signature verification succeeded using certificate");// '{0}'", cert.Subject); //getSubjectX500Principal().getName());
                }
                catch (Exception e)
                {
                    //Console.WriteLine(String.Format("HCERT signature verification failed using certificate '{0}' - {1}",cert, e.Message));
                    continue;
                }

                // OK, before we are done - let's ensure that the HCERT hasn't expired.
                CWT cwt = obj.GetCwt();

                DateTime? expiration = cwt.GetExpiration();
                if (expiration.HasValue)
                {
                    vacProof.ExpirationDate = expiration.Value;
                    if (DateTime.UtcNow.CompareTo(expiration) >= 0)
                    {
                        string message = AppResources.DCCExpired + " {0}";
                        throw new CertificateExpiredException(string.Format(message,expiration.Value.ToShortDateString()));
                    }
                }
                else
                {
                    //Console.WriteLine("Signed HCERT did not contain an expiration time - assuming it is valid");
                }
                DateTime? issuedAt = cwt.GetIssuedAt();
                if(issuedAt.HasValue)
                {
                    vacProof.IssuedDate = issuedAt.Value;
                }
                // OK, everything looks fine - return the DGC
                return cwt.GetDgcV1();
            }

            if (certs.Count<=0)
            {
                throw new CertificateUnknownException(AppResources.NoSigningCertifcateCouldBeFound);
            }
            else
            {
                throw new CertificateValidationException(AppResources.InvalidSigningCertificate);
            }
        }
    }
}
