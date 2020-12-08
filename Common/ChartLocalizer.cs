using JetBrains.Annotations;

namespace Common {
    public class ChartLocalizer {
        [CanBeNull] private static ChartLocalizer _chartLocalizer;
        //[JetBrains.Annotations.NotNull]
        //private readonly Dictionary<string, string> _translations = new Dictionary<string, string>();

        private ChartLocalizer() {
         /*   if (!ShouldTranslate) {
                return;
            }
            if (File.Exists(TranslationFileName)) {
                using (var sr = new StreamReader(TranslationFileName)) {
                    var line = 0;
                    while (!sr.EndOfStream) {
                        var s = sr.ReadLine();
                        line++;
                        if (!string.IsNullOrEmpty(s)) {
                            var arr = s.Split(';');
                            if (arr.Length != 2) {
                                Logger.Error(
                                    TranslationFileName + ": invalid translation entry line " + line + ": " + s);
                                throw new LPGException("invalid translation entry");
                            }
                            if (string.IsNullOrEmpty(arr[0].Trim())) {
                                Logger.Error(
                                    TranslationFileName + ": invalid translation entry 0 " + line + ": " + s);
                                throw new LPGException("invalid translation entry 0");
                            }
                            if (string.IsNullOrEmpty(arr[1].Trim())) {
                                Logger.Error(
                                    TranslationFileName + ": invalid translation entry 1 " + line + ": " + s);
                                throw new LPGException("invalid translation entry 1");
                            }
                            _translations.Add(arr[0].Trim(), arr[1].Trim());
                        }
                    }
                }
            }*/
        }

      /*  [JetBrains.Annotations.NotNull]
        public static string MissingFileName { private get; set; } = @"MissingTranslations.txt";

        private static int MissingTranslationCount { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static bool ShouldTranslate { get; set; }
        [JetBrains.Annotations.NotNull]
        public static string TranslationFileName { get; set; } = @"c:\work\Translations.txt";
        */
        [NotNull]
        public static ChartLocalizer Get() {
            if (_chartLocalizer == null) {
                _chartLocalizer = new ChartLocalizer();
            }
            return _chartLocalizer;
        }

        [NotNull]
        public string GetTranslation([NotNull] string otherText) {
         /*   var key = otherText.Trim();
            if (string.IsNullOrWhiteSpace(key)) {
                return key;
            }
            if (!ShouldTranslate) {
                return key;
            }
            if (!_translations.ContainsKey(key)) {
                using (var sw = new StreamWriter(MissingFileName, true)) {
                    sw.WriteLine(key);
                    MissingTranslationCount++;
                }
                return key;
            }
            return _translations[key];*/
         return otherText;
        }
    }
}