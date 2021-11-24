﻿using System;
using VaccinbevisVerifiering.Services.DGC;
using VaccinbevisVerifiering.Services.DGC.V1;

namespace VaccinbevisVerifiering.Services
{
    /**
    * A DGC representation class.
    *
    * @author Henrik Bengtsson (henrik@sondaica.se)
    * @author Martin Lindström (martin@idsec.se)
    * @author Henric Norlander (extern.henric.norlander@digg.se)
    */
    public class SignedDGC
    {
        public EU_DGC Dgc { get; set; }
        public string IssuingCountry { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime IssuedDate { get; set; }
        public string Message { get; set; }

        public SignedDGC()
        {
        }
    }
}
