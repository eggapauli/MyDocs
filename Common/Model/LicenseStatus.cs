﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Model
{
    public enum LicenseStatus
    {
        Unlocked,
        Locked,
        Error
    }

    public class LicenseStatusException : Exception
    {
        private readonly LicenseStatus licenseStatus;

        public LicenseStatus LicenseStatus { get { return licenseStatus; } }

        public LicenseStatusException(string message, LicenseStatus licenseStatus)
            : this (message, licenseStatus, null)
        {
        }

        public LicenseStatusException(string message, LicenseStatus licenseStatus, Exception inner)
            : base(message, inner)
        {
            this.licenseStatus = licenseStatus;
        }
    }
}
