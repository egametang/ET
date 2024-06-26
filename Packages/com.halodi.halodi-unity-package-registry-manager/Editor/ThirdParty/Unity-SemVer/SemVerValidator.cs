using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Artees.UnitySemVer
{
    internal class SemVerValidator
    {
        private List<string> _errors;
        private SemVer _corrected;

        public SemVerValidationResult Validate(SemVer semVer)
        {
            _errors = new List<string>();
            _corrected = semVer.Clone();
            ValidatePreRelease(semVer);
            ValidateBuild(semVer);
            return new SemVerValidationResult(_errors.AsReadOnly(), _corrected.Clone());
        }

        private void ValidatePreRelease(SemVer semVer)
        {
            var identifiers = ValidateIdentifiers(semVer.preRelease);
            identifiers = ValidateLeadingZeroes(identifiers);
            var joined = JoinIdentifiers(identifiers);
            _corrected.preRelease = joined;
        }

        private void ValidateBuild(SemVer semVer)
        {
            if (SemVerAutoBuild.Instances[semVer.autoBuild] is SemVerAutoBuild.ReadOnly) return;
            var identifiers = ValidateIdentifiers(semVer.Build);
            var joined = JoinIdentifiers(identifiers);
            _corrected.Build = joined;
        }

        private string[] ValidateIdentifiers(string identifiers)
        {
            if (string.IsNullOrEmpty(identifiers)) return new string[0];
            var separators = new[] {SemVer.IdentifiersSeparator};
            var strings = identifiers.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length < identifiers.Count(c => c == SemVer.IdentifiersSeparator) + 1)
            {
                _errors.Add(SemVerErrorMessage.Empty);
            }

            for (var i = 0; i < strings.Length; i++)
            {
                var raw = strings[i];
                var corrected = Regex.Replace(raw, "[^0-9A-Za-z-]", "-");
                if (string.Equals(raw, corrected)) continue;
                _errors.Add(SemVerErrorMessage.Invalid);
                strings[i] = corrected;
            }

            return strings;
        }

        private string[] ValidateLeadingZeroes(IList<string> identifiers)
        {
            var length = identifiers.Count;
            var corrected = new string[length];
            for (var i = 0; i < length; i++)
            {
                var identifier = identifiers[i];
                var isNumeric = int.TryParse(identifier, out var numericIdentifier);
                if (isNumeric && identifier.StartsWith("0") && identifier.Length > 1)
                {
                    corrected[i] = numericIdentifier.ToString();
                    _errors.Add(SemVerErrorMessage.LeadingZero);
                }
                else
                {
                    corrected[i] = identifier;
                }
            }

            return corrected;
        }

        private static string JoinIdentifiers(string[] identifiers)
        {
            return string.Join(SemVer.IdentifiersSeparator.ToString(), identifiers);
        }
    }
}