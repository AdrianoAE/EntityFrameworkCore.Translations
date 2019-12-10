using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AdrianoAE.EntityFrameworkCore.Translations
{
    public class TranslationCSharpHelper : CSharpHelper
    {
        public TranslationCSharpHelper([NotNull] IRelationalTypeMappingSource relationalTypeMappingSource) : base(relationalTypeMappingSource)
        { }

        public string TranslationLanguageConfiguration(object value)
        {
            if (value is IEnumerable<LanguageTableConfiguration> constraints)
            {
                var teste = constraints.Select(x => "").ToArray();
                return Literal(teste);
            }

            return base.UnknownLiteral(value);
        }

        public override string UnknownLiteral(object value)
        {
            if (value is IEnumerable<LanguageTableConfiguration>)
            {
                return TranslationLanguageConfiguration(value);
            }

            return base.UnknownLiteral(value);
        }
    }
}
