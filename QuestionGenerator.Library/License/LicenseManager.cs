﻿namespace QuestionGenerator.Library.License
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class LicenseManager
    {
        /// <summary>
        /// Generates the serial key.
        /// </summary>
        public static string GetDiskSerialKey()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
            {
                if (searcher.Get().OfType<ManagementObject>().FirstOrDefault() is ManagementObject mo)
                {
                    return mo["SerialNumber"].ToString();
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether [is license valid].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is license valid]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="InvalidDataException">
        /// </exception>
        public static bool IsLicenseValid(out LicenseInfo returnLicenseInfo)
        {
            var licenseFile = ConfigurationManager.AppSettings["LicenseFile"].ToString();
            if (!File.Exists(licenseFile))
            {
                throw new FileNotFoundException();
            }

            using (var stream = File.Open(Path.GetFullPath(licenseFile), FileMode.Open, FileAccess.ReadWrite))
            {
                var formatter = new BinaryFormatter();
                var rawObject = formatter.Deserialize(stream);
                if (!(rawObject is LicenseInfo licenseInfo))
                {
                    throw new InvalidDataException();
                }

                returnLicenseInfo = licenseInfo;
                var validDays = (licenseInfo.Expiry - DateTime.UtcNow.Date).TotalDays;
                if (validDays < 0)
                {
                    return false;
                }

                var serialKey = GetDiskSerialKey();
                if (serialKey == null)
                {
                    throw new InvalidDataException();
                }

                if (string.IsNullOrWhiteSpace(licenseInfo.MachineId))
                {
                    licenseInfo.MachineId = serialKey;
                    formatter.Serialize(stream, licenseInfo);
                    stream.Flush();
                    stream.Close();

                    return true;
                }

                return string.Equals(serialKey, licenseInfo.MachineId, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Temporaries the write license file.
        /// </summary>
        public static void TempWriteLicenseFile()
        {
            var licenseFile = ConfigurationManager.AppSettings["LicenseFile"].ToString();
            using (var stream = File.Open(Path.GetFullPath(licenseFile), FileMode.Create, FileAccess.ReadWrite))
            {
                var formatter = new BinaryFormatter();
                var serialKey = GetDiskSerialKey();
                var licenseInfo = new LicenseInfo
                {
                    Expiry = new DateTime(2020, 12, 31),
                    Key = Guid.NewGuid().ToString(),
                    MachineId = GetDiskSerialKey(),
                    Version = "1.0",
                };

                formatter.Serialize(stream, licenseInfo);
                stream.Flush();
                stream.Close();
            }
        }
    }
}
