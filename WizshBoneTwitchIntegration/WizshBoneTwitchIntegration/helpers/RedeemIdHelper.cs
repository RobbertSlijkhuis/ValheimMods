using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Gives every redeem its permanent <see cref="RedeemData.id"/>. A redeem is identified by it, not by its title:
    /// the title is what viewers see and changes with a rename or a new prefix.
    /// </summary>
    internal static class RedeemIdHelper
    {
        public static string NewId()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Fills in a missing id and replaces a repeated one (a hand-copied block in profile.yaml). The ids that are
        /// already there and unique are kept as they are. A redeem from before ids existed gets one derived from
        /// its title, so it is the same on every load until the profile is saved with it - a profile that is
        /// restored from its stock file on every read (a locked one) therefore keeps stable ids too.
        /// </summary>
        public static void EnsureIds(List<RedeemData> redeems)
        {
            HashSet<string> used = new HashSet<string>();
            List<RedeemData> needId = new List<RedeemData>();

            foreach (RedeemData redeem in redeems)
            {
                if (string.IsNullOrEmpty(redeem.id) || !used.Add(redeem.id))
                    needId.Add(redeem);
            }

            foreach (RedeemData redeem in needId)
            {
                string id = FromTitle(redeem.title);

                if (!used.Add(id))
                {
                    id = NewId();
                    used.Add(id);
                }

                redeem.id = id;
            }
        }

        private static string FromTitle(string title)
        {
            using (MD5 md5 = MD5.Create())
            {
                // Case-sensitive on purpose: "Foo" and "foo" in an old file must not end up with the same id.
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(title ?? ""));
                StringBuilder id = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                    id.Append(b.ToString("x2"));

                return id.ToString();
            }
        }
    }
}
