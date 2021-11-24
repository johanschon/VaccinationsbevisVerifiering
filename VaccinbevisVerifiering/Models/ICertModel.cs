﻿using System;
namespace VaccinbevisVerifiering.Models
{
    public interface ICertModel
    {
        public CertType Type { get; set; }
        public string Header { get; set; }
        public string Info { get; set; }
        public void CreateHeaderAndInfo();

    }
}
