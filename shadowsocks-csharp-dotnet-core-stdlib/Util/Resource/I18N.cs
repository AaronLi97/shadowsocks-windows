using System.Collections.Generic;

namespace Shadowsocks.Std.Util.Resource
{
    /// <summary>
    /// Obtain I18N resources (<see cref="I18NResource"/>) and convert to the target data structure ( <see cref="GetI18N"/> => <see cref="I18N._strings"/>)
    /// </summary>
    public interface IGetI18N
    {
        /// <summary>
        /// Define a delegate to get I18N content.
        /// </summary>
        /// <returns>I18N content</returns>
        public delegate string I18NResource();

        /// <summary>
        /// Reset resource delegation.
        /// Need to call <see cref="GetI18N"/> again.
        /// </summary>
        /// <param name="resource">I18N resource delegation</param>
        public void SetResources(I18NResource resource);

        /// <summary>
        /// Add new I18N content to <see cref="strings"/>
        /// </summary>
        /// <param name="strings">I18N content</param>
        public void GetI18N(ref Dictionary<string, string> strings);
    }

    public class I18N
    {
        public const string I18N_FILE = "i18n.csv";

        private static Dictionary<string, string> _strings = new Dictionary<string, string>();

        private readonly IGetI18N i18N;

        public I18N(IGetI18N i18N)
        {
            this.i18N = i18N;
            this.i18N.GetI18N(ref _strings);
            this.i18N.SetResources(GetI18N);
            this.i18N.GetI18N(ref _strings);
        }

        private static string GetI18N() => Resources.i18n_csv;

        public static string GetString(string key, params object[] args) => string.Format(_strings.TryGetValue(key.Trim(), out var value) ? value : key, args);
    }
}