using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime;
using Fpx.Generated;
using static Fpx.Generated.FpxParser;
using System.Reflection;
using Antlr4.Runtime.Tree;

namespace Fpx
{
    public class FpxVisitor : FpxParserBaseVisitor<object>
    {
        private const int STRING_LIST_VALUE_LIMIT = 256;
        private const int STRING_VALUE_LIMIT = 256;
        private const char COMMA = ',';
        private const char QUOTE = '\"';
        private const char SEMICOL = ';';

        private string primaryLang;
        private List<string> allLangs;
        private Dictionary<string, List<string>> heading;
        private Dictionary<string, List<string>> stub;


        private readonly List<string> Errors;

        public FpxVisitor(List<string> _Errors)
        {
            this.Errors = _Errors;
            this.heading = new Dictionary<string, List<string>>();
            this.stub = new Dictionary<string, List<string>>();
        } // FpxVisitor

        public override object VisitPxfile([NotNull] FpxParser.PxfileContext context)
        {
            // get primary lang
            var lang = context.LANGSLO().GetText();
            this.primaryLang = lang.Substring(1, lang.Length-2);
            return base.VisitPxfile(context);
        } // VisitPxfile


        public override object VisitStringlist([NotNull] StringlistContext context)
        {
            // prevent StackOverflowException as recursion exceeds max depth with long
            // string lists (e.g. VALUES, CODES)
            return null;
        } // VisitStringlist


        public override object VisitMultistringlist([NotNull] MultistringlistContext context)
        {
            // prevent StackOverflowException as recursion exceeds max depth with long
            // string lists (e.g. VALUES, CODES)
            return null;
        } // VisitMultistringlist


        public override object VisitLanguages([NotNull] LanguagesContext context)
        {

            var langs = context.LANGSVAL().GetText().Split(new char[] { FpxVisitor.COMMA, FpxVisitor.QUOTE}, 
                StringSplitOptions.RemoveEmptyEntries);
            if ((langs == null) || (langs.Length == 0))
            {
                this.Errors.Add(context.LANGUAGES().GetText() + ",  line" + context.Start.Line + ":" +
                    context.Start.Column + " px file has to be multilingual");
            } // if
            else
            {
                this.allLangs = langs.ToList();
            } // else
            return base.VisitLanguages(context);
        } // VisitLanguages


        public override object VisitCreationdate([NotNull] CreationdateContext context)
        {
            if (context.DATETIMEVAL() != null)
                this.checkStringValueLimit(context.CREATIONDATE().GetText(), context.Start.Line,
                    context.Start.Column, context.DATETIMEVAL().GetText(), 
                    FpxVisitor.STRING_VALUE_LIMIT);
            return base.VisitCreationdate(context);
        } // VisitCreationdate


        public override object VisitNextupdate([NotNull] NextupdateContext context)
        {
            if (context.DATETIMEVAL() != null)
                this.checkStringValueLimit(context.NEXTUPDATE().GetText(), context.Start.Line,
                    context.Start.Column, context.DATETIMEVAL().GetText(), 
                    FpxVisitor.STRING_VALUE_LIMIT);
            return base.VisitNextupdate(context);
        } // VisitNextupdate


        public override object VisitPxserver([NotNull] PxserverContext context)
        {
            if (context.multistring() != null)
                this.checkStringValueLimit(context.PXSERVER().GetText(), context.Start.Line,
                    context.Start.Column, context.multistring().GetText(), 
                    FpxVisitor.STRING_VALUE_LIMIT);
            return base.VisitPxserver(context);
        } // VisitPxServer


        public override object VisitCodes([NotNull] CodesContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var code = context.code();
                var keyLangsList = this.collectLanguagesForKeywordNonRec(code, 
                    "VisitCode", true);
                this.checkLangs(code[0].CODES().GetText(), code[0].Start.Line, 
                    code[0].Start.Column, keyLangsList, true);
            } // if

            return null;
        } // VisitCodes


        public override object VisitCode([NotNull] CodeContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "CODES", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.CODES().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            string[] codeVals = context.stringlist().GetText().Split(FpxVisitor.COMMA);
            this.checkStringListValueLimit(context.CODES().GetText(), context.Start.Line,
                context.Start.Column, codeVals, FpxVisitor.STRING_LIST_VALUE_LIMIT);

            return keyLangPair;
        } // VisitCode


        private void checkStringListValueLimit(string _keyword, int _startLine, 
            int _startColumn, string[] _list, int _limit)
        {
            foreach (var strVal in _list)
                this.checkStringValueLimit(_keyword, _startLine, _startColumn, 
                    strVal, _limit);
        } // checkStringListValueLimit


        private void checkStringValueLimit(string _keyword, int _startLine,
            int _startColumn, string _str, int _limit)
        {
                if (_str.Length-2 > _limit)
                    this.Errors.Add(_keyword + " starting on " + _startLine + ":" + 
                        _startColumn + " has " + _str + " longer than " + 
                        FpxVisitor.STRING_LIST_VALUE_LIMIT);
        } // checkStringValueLimit


        public override object VisitDirectorypath([NotNull] DirectorypathContext context)
        {
            if (context.multistring() != null)
                this.checkStringValueLimit(context.DIRECTORYPATH().GetText(), context.Start.Line,
                    context.Start.Column, context.multistring().GetText(), FpxVisitor.STRING_VALUE_LIMIT);
            return base.VisitDirectorypath(context);
        } // VisitDirectorypath


        public override object VisitUpdatefrequency([NotNull] UpdatefrequencyContext context)
        {
            if (context.multistring() != null)
                this.checkStringValueLimit(context.UPDATEFREQUENCY().GetText(), context.Start.Line,
                    context.Start.Column, context.multistring().GetText(), FpxVisitor.STRING_VALUE_LIMIT);
            return base.VisitUpdatefrequency(context);
        } // VisitUpdatefrequency


        public override object VisitTableid([NotNull] TableidContext context)
        {
            if (context.multistring() != null)
                this.checkStringValueLimit(context.TABLEID().GetText(), context.Start.Line, 
                    context.Start.Column, context.multistring().GetText(), 
                    FpxVisitor.STRING_VALUE_LIMIT);

            return base.VisitTableid(context);
        } // VisitTableid


        public override object VisitSynonym([NotNull] SynonymContext context)
        {
            if (context.multistring() != null)
                this.checkStringValueLimit(context.SYNONYMS().GetText(), context.Start.Line, 
                    context.Start.Column, context.multistring().GetText(), 
                    FpxVisitor.STRING_VALUE_LIMIT);

            return base.VisitSynonym(context);
        } // VisitSynonym


        public override object VisitSubjectareas([NotNull] SubjectareasContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var sa = context.subjectarea();
//                var keyLangsList = this.collectLanguagesForKeyword(context, "subjectarea", 
//                    "VisitSubjectarea", "subjectareas", false);
//                this.checkLangs(sa.SUBJECTAREA().GetText(), sa.Start.Line, 
//                    sa.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(sa, 
                    "VisitSubjectarea", false);
                this.checkLangs(sa[0].SUBJECTAREA().GetText(), sa[0].Start.Line, 
                    sa[0].Start.Column, keyLangsList, false);
            } // if

            return null;
        } // VisitSubjectareas


        public override object VisitSubjectarea([NotNull] SubjectareaContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "SUBJECTAREA", 
                "LANGDECLAR", null);
            this.checkStringValueLimit(context.SUBJECTAREA().GetText(), context.Start.Line, 
                context.Start.Column, context.str().GetText(), 
                FpxVisitor.STRING_VALUE_LIMIT);

            return keyLangPair;
        } // VisitSubjectarea


        public override object VisitContents([NotNull] ContentsContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var content = context.content();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "content", 
                //    "VisitContent", "contents", false);
                //this.checkLangs(content.CONTENTS().GetText(), content.Start.Line, 
                //    content.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(content,
                    "VisitContent", false);
                this.checkLangs(content[0].CONTENTS().GetText(), content[0].Start.Line, 
                    content[0].Start.Column, keyLangsList, false);
            } // if

            return null;
        } // VisitContents


        private Dictionary<string, List<string>> collectLanguagesForKeywordNonRec(
            object[] _childContexts, string _visitTarget, bool _restrictToDimensions)
        {
            KeyValuePair<string, string> keyValue;
            Dictionary<string, List<string>> keyLangsList = null;

            // iterate through all children - terminal nodes
            foreach (var cContext in _childContexts)
            {
                var visitorClass = this.GetType();
                MethodInfo visitTargetMI = visitorClass.GetMethod(_visitTarget);
                
                // e.g. VisitContent(context.content())
                keyValue = (KeyValuePair<string, string>)
                    visitTargetMI.Invoke(this, new object[] { cContext });

                // language specific dimension normalization
                bool skipDim = false;
                keyValue = this.dimLangNormalization(keyValue, _restrictToDimensions, 
                    ref skipDim);

                // add a language to the dictionary if it is not flagged as skipped
                if (!skipDim)
                {
                    if (keyLangsList == null)
                        keyLangsList = new Dictionary<string, List<string>>();
                    this.addOrCreateToDictionary(keyLangsList, keyValue);
                } // if
            } // foreach

            return keyLangsList;
        } // collectLanguagesForKeywordNonRec


        private Dictionary<string, List<string>> collectLanguagesForKeyword(object _context,
            string _target, string _visitTarget, string _iterator, bool _restrictToDimensions)
        {
            // get context class and method via reflection
            var typeContext = _context.GetType();
            var visitorClass = this.GetType();
            MethodInfo targetMI = typeContext.GetMethod(_target);
            MethodInfo visitTargetMI = visitorClass.GetMethod(_visitTarget);
            MethodInfo iteratorMI = typeContext.GetMethod(_iterator);
            
            KeyValuePair<string, string> keyValue;
            Dictionary<string, List<string>> keyLangsList = null;
            // e.g. _context.content() to the context for e.g. content keyword
            var targetContext = targetMI.Invoke(_context, null);
            if (targetContext != null)
            {
                // e.g. VisitContent(context.content())
                keyValue = (KeyValuePair<string, string>)
                    visitTargetMI.Invoke(this, new object[] { targetContext });

                // language specific dimension normalization
                bool skipDim = false;
                keyValue = this.dimLangNormalization(keyValue, _restrictToDimensions, 
                    ref skipDim);

                // check if descendant exists: e.g. context.contents() != null
                var iteratorContext = iteratorMI.Invoke(_context, null);
                if (iteratorContext != null)
                {
                    // recurse one level lower
                    keyLangsList = (Dictionary<string, List<string>>)
                        this.collectLanguagesForKeyword(iteratorContext, _target, 
                        _visitTarget, _iterator, _restrictToDimensions);
                } // if

                // add a language to the dictionary if it is not flagged as skipped
                if (!skipDim)
                {
                    if (keyLangsList == null)
                        keyLangsList = new Dictionary<string, List<string>>();
                    this.addOrCreateToDictionary(keyLangsList, keyValue);
                } // if
            } // if

            return keyLangsList;
        } // collectLanguagesForKeyword


        /// <summary>
        /// Gets the same dimension (origin dimension) for the primary language
        /// and adds the language for the origin dimension so we are able
        /// to see all languages for each keyword+originDimension.
        /// </summary>
        /// <returns></returns>
        private KeyValuePair<string, string> dimLangNormalization(
            KeyValuePair<string, string> _keyValue, 
            bool _restrictToDimensions, ref bool _skipDim)
        {
            // if key, value are empty (in case of invalid dimension) then do not look for 
            if (_keyValue.Equals(default(KeyValuePair<string, string>)))
            {
                _skipDim = true;
                return _keyValue;
            } // if

            // get dimension value from (dimension_value)
            string dim = this.extractDim(_keyValue.Key);
            if (!string.IsNullOrEmpty(dim))
            {
                string dimOrigin = this.getOriginDimDeclar(dim, _keyValue.Value);
                if (!string.IsNullOrEmpty(dimOrigin))
                {
                    // replace the given key with the origin dimension
                    string newKey = _keyValue.Key.Replace(dim, dimOrigin);
                    _keyValue = new KeyValuePair<string, string>(newKey, _keyValue.Value);
                } // if
                else if (_restrictToDimensions)
                {
                    // _restrictToDimensions means that the dim contains dimension
                    // declaration. dim can otherwise contain other declarations in
                    // the form of ("val1", "val2", ..., "valn") which is not a dimensio
                    // but some other form of declaration. When flag is set to true this
                    // means we are dealing with dimension declaration, which must have 
                    // the origin dimension. If it does not have this means that this
                    // dimension is not valid: not declared in STUB, HEADING for the
                    // specified language and is therefore not posible to find the same
                    // dimension(origin) the primary language. That is why we signal the
                    // outside world to skip dealing with the invalid dimension declaration.
                    _skipDim = true;
                } // else if
            } // if

            return _keyValue;
        } // dimLangNormalization


        /// <summary>
        /// Extracts dimension from string like (dimension) => omit the parenthesis
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        private string extractDim(string _str)
        {
            if (_str.Contains("(") && _str.Contains(")"))
            {
                int s = _str.IndexOf("(");
                int e = _str.IndexOf(")");
                return _str.Substring(s + 1, e - s - 1);
            } // if

            return null;
        } // extractDim


        /// <summary>
        /// Returns the dimension for the primary language out of 
        /// the dimension for another language.
        /// </summary>
        /// <param name="_dim"></param>
        /// <param name="_lang"></param>
        /// <returns></returns>
        private string getOriginDimDeclar(string _dim, string _lang)
        {
            Dictionary<string, List<string>> dims = null;
            if (this.heading[_lang].Contains(_dim))
                dims = this.heading;
            else if (this.stub[_lang].Contains(_dim))
                dims = this.stub;

            if (dims != null)
            {
                int dimIndex = dims[_lang].IndexOf(_dim);
                return dims[this.primaryLang][dimIndex];
            } // if

            return null;
        } // getOriginDimension


        private void addOrCreateToDictionary(Dictionary<string,List<string>> _dict,
            KeyValuePair<string, string> _keyVal)
        {
            if (_dict.ContainsKey(_keyVal.Key))
                _dict[_keyVal.Key].Add(_keyVal.Value);
            else
                _dict.Add(_keyVal.Key, new List<string> {
                    _keyVal.Value
                });
        } // addOrCreateToDictionary


        private void checkLangs(string _keyword, int _startLine, int _startColumn, 
            Dictionary<string, List<string>> keyLangsList, bool _multiplicity)
        {
            // if there are no language definitions keyword[lang](DIMDECLAR) 
            // and therefore not have keyLangList = {"keywordDIMDECLAR": [lang1, lang2, ...]}
            if (keyLangsList == null)
            {
                this.Errors.Add(_keyword + " declarations starting on line " +
                    _startLine + ":" + _startColumn +
                    " have invalid language declarations.");
                return;
            } // if

            // if we are not dealing with multiple dimension declarations 
            // of the same keyword (multuplicity of 1) then there shoud be only
            // one entry in a Dictionary such as keyword: [lang1, lang2, ..., langn]
            if ((!_multiplicity) && (keyLangsList.Count != 1))
            {
                this.Errors.Add(_keyword + " declarations starting on line " +
                    _startLine + ":" + _startColumn +
                    " must have multiplicity of 1.");
                return;
            } // if

            // go through all declarations of keyword and check if each
            // declaration has all languages declared in LANGUAGES keyword
            foreach (var key in keyLangsList.Keys)
            {
                bool valid = this.chekIfOnlyValidLangs(_keyword, _startLine, 
                    _startColumn, keyLangsList[key]);
                if (!valid)
                    break;
            } // foreach
        } // checkLangs


        /// <summary>
        /// Check to see if languages in _list are the ones declared in LANGUAGES keyword.
        /// This requires to compare this.allLangs and _list Lists. Clone both objects at
        /// the beginning and remove the matching entries. If at the end either of them 
        /// has any entries left this means invalid / insufficient language specification.
        /// </summary>
        /// <param name="_keyword"></param>
        /// <param name="_startLine"></param>
        /// <param name="_startColumn"></param>
        /// <param name="_list"></param>
        /// <returns></returns>
        private bool chekIfOnlyValidLangs(string _keyword, int _startLine, 
            int _startColumn, List<string> _list)
        {
            var allLangsClone = this.allLangs.ToList();
            var listLangsClone = _list.ToList();
            foreach (var lang in this.allLangs)
            {
                if (listLangsClone.Contains(lang))
                    allLangsClone.Remove(lang);
                listLangsClone.Remove(lang);
                if ((allLangsClone.Count == 0) || (listLangsClone.Count == 0))
                    break;
            } // foreach

            if ((allLangsClone.Count > 0) || (listLangsClone.Count > 0))
            {
                this.Errors.Add(_keyword + " declarations starting on line " +
                    _startLine + ":" + _startColumn +
                    " have inconsistent language specifications.");
                return false;
            } // if
            return true;
        } // chekIfOnlyValidLangs


        public override object VisitContent([NotNull] ContentContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "CONTENTS", 
                "LANGDECLAR", null);
            this.checkStringValueLimit(context.CONTENTS().GetText(), context.Start.Line, 
                context.Start.Column, context.str().GetText(), 
                FpxVisitor.STRING_VALUE_LIMIT);

            return keyLangPair;
        } // VisitContent


        public override object VisitUnitss([NotNull] UnitssContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var units = context.units();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "units", 
                //    "VisitUnits", "unitss", false);
                //this.checkLangs(units.UNITS().GetText(), units.Start.Line, 
                //    units.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(units,
                    "VisitUnits", false);
                this.checkLangs(units[0].UNITS().GetText(), units[0].Start.Line, 
                    units[0].Start.Column, keyLangsList, false);
            } // if

            return null;
        } // VisitUnitss


        public override object VisitUnits([NotNull] UnitsContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "UNITS", 
                "LANGDECLAR", null);
            this.checkStringValueLimit(context.UNITS().GetText(), context.Start.Line, 
                context.Start.Column, context.str().GetText(), 
                FpxVisitor.STRING_VALUE_LIMIT);

            return keyLangPair;
        } // VisitUnits


        /// <summary>
        /// Checks if the given language is one defined in LANGUAGES keyword.
        /// </summary>
        /// <param name="_keyword"></param>
        /// <param name="_startLine"></param>
        /// <param name="_startColumn"></param>
        /// <param name="_lang"></param>
        private void checkValidLang(string _keyword, int _startLine,
            int _startColumn, string _lang)
        {
            if (!this.allLangs.Contains(_lang))
            {
                this.Errors.Add(_keyword + " line" + 
                    _startLine + ":" + _startColumn + 
                    " invalid language specification.");
            } // if
        } // checkValidLang


        public override object VisitStubs([NotNull] StubsContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var stub = context.stub();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "stub", 
                //    "VisitStub", "stubs", false);
                //this.checkMatchingDictVals(FpxLexer.DefaultVocabulary.GetSymbolicName(FpxLexer.STUB), 
                //    stub.Start.Line, stub.Start.Column, this.stub);
                //this.checkLangs(stub.STUB().GetText(), stub.Start.Line, 
                //    stub.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(stub,
                    "VisitStub", false);
                this.checkMatchingDictVals(FpxLexer.DefaultVocabulary.GetSymbolicName(FpxLexer.STUB),
                    stub[0].Start.Line, stub[0].Start.Column, this.stub);
                this.checkLangs(stub[0].STUB().GetText(), stub[0].Start.Line,
                    stub[0].Start.Column, keyLangsList, false);
            } // if
            return null;
        } // VisitStubs


        public override object VisitStub([NotNull] StubContext context)
        {
            var keyLangPair = (KeyValuePair<string, string>)
                this.visitAndExtractLangFromKeyword(context, "STUB", 
                "LANGDECLAR", null);
            var stubStringList = context.stringlist().GetText().Split(FpxVisitor.COMMA);
            this.addStringListToDictionary(this.stub, keyLangPair.Value, stubStringList);
            this.checkStringListValueLimit(context.STUB().GetText(), context.Start.Line, 
                context.Start.Column, stubStringList, 
                FpxVisitor.STRING_LIST_VALUE_LIMIT);

            return keyLangPair;
        } // VisitStub


        private void addStringListToDictionary(Dictionary<string, List<string>> _dict,
            string _key, string[] _strList)
        {
            if (!_dict.ContainsKey(_key))
                _dict[_key] = new List<string>();

            foreach (var val in _strList)
            {
                _dict[_key].Add(val);
            } // foreach
        } // addStringListToDictionary


        public override object VisitHeadings([NotNull] HeadingsContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var heading = context.heading();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "heading", 
                //    "VisitHeading", "headings", false);
                //this.checkMatchingDictVals(FpxLexer.DefaultVocabulary.GetSymbolicName(FpxLexer.HEADING), 
                //    heading.Start.Line, heading.Start.Column, this.heading);
                //this.checkLangs(heading.HEADING().GetText(), heading.Start.Line, 
                //    heading.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(heading, 
                    "VisitHeading", false);
                this.checkMatchingDictVals(FpxLexer.DefaultVocabulary.GetSymbolicName(FpxLexer.HEADING),
                    heading[0].Start.Line, heading[0].Start.Column, this.heading);
                this.checkLangs(heading[0].HEADING().GetText(), heading[0].Start.Line,
                    heading[0].Start.Column, keyLangsList, false);
            } // if

            return base.VisitHeadings(context);
        } // VisitHeadings


        /// <summary>
        /// Checks if Dictionary like this.stub or this.heading have the same
        /// number of dimensions like for the primary language, which is the first
        /// key.
        /// </summary>
        /// <param name="_keyword"></param>
        /// <param name="_startLine"></param>
        /// <param name="_startColumn"></param>
        /// <param name="_dict"></param>
        private void checkMatchingDictVals(string _keyword, int _startLine, 
            int _startColumn, Dictionary<string, List<string>> _dict)
        {
            if (_dict.Count > 0)
            {
                var keys = _dict.Keys;
                for (int i = 1; i < keys.Count; i++)
                {
                    if (_dict[keys.ElementAt(i)].Count != _dict[keys.ElementAt(0)].Count)
                    {
                        this.Errors.Add(_keyword + " declarations starting on " + _startLine + ":" + 
                            _startColumn + " have inconsistent values.");
                    } // if
                } // for
            } // if
        } // checkMatchingDictVals


        public override object VisitHeading([NotNull] HeadingContext context)
        {
            var keyLangPair = (KeyValuePair<string, string>)
                this.visitAndExtractLangFromKeyword(context, "HEADING", 
                "LANGDECLAR", null);
            var stubStringList = context.stringlist().GetText().Split(FpxVisitor.COMMA);
            this.addStringListToDictionary(this.heading, keyLangPair.Value, stubStringList);
            this.checkStringListValueLimit(context.HEADING().GetText(), context.Start.Line, 
                context.Start.Column, context.stringlist().GetText().Split(FpxVisitor.COMMA), 
                FpxVisitor.STRING_LIST_VALUE_LIMIT);

            return keyLangPair;
        } // VisitHeading


        public override object VisitContvariables([NotNull] ContvariablesContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var contvariable = context.contvariable();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "contvariable", 
                //    "VisitContvariable", "contvariables", false);
                //this.checkLangs(contvariable.CONTVARIABLE().GetText(), contvariable.Start.Line, 
                //    contvariable.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(contvariable,
                    "VisitContvariable", false);
                this.checkLangs(contvariable[0].CONTVARIABLE().GetText(), contvariable[0].Start.Line, 
                    contvariable[0].Start.Column, keyLangsList, false);
            } // if

            return null;
        } // VisitContvariables


        public override object VisitContvariable([NotNull] ContvariableContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "CONTVARIABLE", 
                "LANGDECLAR", null);
            this.checkStringValueLimit(context.CONTVARIABLE().GetText(), context.Start.Line, 
                context.Start.Column, context.multistring().GetText(), 
                FpxVisitor.STRING_VALUE_LIMIT);

            return keyLangPair;
        } // VisitContvariable


        public override object VisitPxvaluess([NotNull] PxvaluessContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var pxvalues = context.pxvalues();
//                var keyLangsList = this.collectLanguagesForKeyword(context, "pxvalues", 
//                    "VisitPxvalues", "pxvaluess", true);
//                this.checkLangs(pxvalues.VALUES().GetText(), pxvalues.Start.Line, 
//                    pxvalues.Start.Column, keyLangsList, true);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(pxvalues, 
                    "VisitPxvalues", true);
                this.checkLangs(pxvalues[0].VALUES().GetText(), pxvalues[0].Start.Line, 
                    pxvalues[0].Start.Column, keyLangsList, true);
            } // if

            return null;
        } // VisitPxvaluess


        public override object VisitPxvalues([NotNull] PxvaluesContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "VALUES", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.VALUES().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            string[] timevalVals = context.multistringlist().GetText().Split(FpxVisitor.COMMA);
            this.checkStringListValueLimit(context.VALUES().GetText(), 
                context.Start.Line, context.Start.Column, timevalVals, 
                FpxVisitor.STRING_LIST_VALUE_LIMIT);

            return keyLangPair;
        } // VisitPxvalues


        public override object VisitTimevals([NotNull] TimevalsContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var timeval = context.timeval();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "timeval", 
                //    "VisitTimeval", "timevals", true);
                //this.checkLangs(timeval.TIMEVAL().GetText(), timeval.Start.Line, 
                //    timeval.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(timeval,
                    "VisitTimeval", true);
                this.checkLangs(timeval[0].TIMEVAL().GetText(), timeval[0].Start.Line, 
                    timeval[0].Start.Column, keyLangsList, false);
            } // if

            return null;
        } // VisitTimevals


        public override object VisitTimeval([NotNull] TimevalContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "TIMEVAL", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.TIMEVAL().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            string[] timevalVals = context.stringlist().GetText().Split(FpxVisitor.COMMA);
            this.checkStringListValueLimit(context.TIMEVAL().GetText(), 
                context.Start.Line, context.Start.Column, timevalVals, 
                FpxVisitor.STRING_LIST_VALUE_LIMIT);

            return keyLangPair;
        } // VisitTimeval


        /// <summary>
        /// Checks if the dimension is declared either in STUB or HEADING depending
        /// on the language.
        /// </summary>
        /// <param name="_keyword"></param>
        /// <param name="_line"></param>
        /// <param name="_column"></param>
        /// <param name="_lang"></param>
        /// <param name="_dim"></param>
        /// <returns></returns>
        private bool checkDimension(string _keyword, int _line, int _column,
            string _lang, string _dim)
        {
            if (this.heading.ContainsKey(_lang) && this.stub.ContainsKey(_lang))
            {
                if (this.heading[_lang].Contains(_dim))
                    return true;
                if (this.stub[_lang].Contains(_dim))
                    return true;
            } // if

            this.Errors.Add(_keyword + " on line " + _line + ":" + 
                _column + " has invalid dimension value.");

            return false;
        } // checkDimension


        /// <summary>
        /// Gets the keyword parse context, descents the parse tree, extracts languages 
        /// for this keyword + dimension. This produces the Dictionary like
        /// {
        ///     keywordDIMDECLAR1: [lang1, lang2, ...] 
        ///     ...
        /// }
        /// </summary>
        /// <param name="_context"></param>
        /// <param name="_keywordFuncName"></param>
        /// <param name="_langDeclarFuncName"></param>
        /// <param name="_dimDeclarFuncName"></param>
        /// <returns></returns>
        private KeyValuePair<string, string> visitAndExtractLangFromKeyword(ParserRuleContext _context, 
            string _keywordFuncName, string _langDeclarFuncName, string _dimDeclarFuncName)
        {
            // e.g. context.CONTVARIABLE()
            var typeContext = _context.GetType();
            MethodInfo keywordFuncMI = typeContext.GetMethod(_keywordFuncName);
            var keywordTerminalNode = (ITerminalNode)keywordFuncMI.Invoke(_context, null);
            string keyword = keywordTerminalNode.GetText();
            int sLine = _context.Start.Line;
            int sCol = _context.Start.Column;
            // e.g. context.LANGDECLAR()
            MethodInfo langDeclarMI = typeContext.GetMethod(_langDeclarFuncName);
            var langTerminalNode = (ITerminalNode)langDeclarMI.Invoke(_context, null);

            // extract the language declaration
            string langValue;
            if (langTerminalNode != null)
            {
                langValue = langTerminalNode.GetText();
                langValue = langValue.Substring(1, langValue.Length - 2);
                // check to see if extracted lang is one of declared 
                // in LANGUAGES (this.allLangs)
                this.checkValidLang(keyword, sLine,
                    sCol, langValue);
            } // if
            else
            {
                langValue = this.primaryLang;
            } // else

            // extract the dimension declaration e.g. context.DIMDECLAR()
            MethodInfo dimDeclarMI = (_dimDeclarFuncName != null) ? 
                typeContext.GetMethod(_dimDeclarFuncName) : null;
            // if the keyword has dimension declaration extract if and
            // compose key in Dictionary as keywordDIMDECLAR
            if (dimDeclarMI != null)
            {
                var dimTerminalNode = (ITerminalNode)dimDeclarMI.Invoke(_context, null);
                keyword += dimTerminalNode.GetText();
            } // if

            return new KeyValuePair<string, string>(keyword, langValue);
        } // visitAndExtractLangFromKeyword


        public override object VisitDomains([NotNull] DomainsContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var domain = context.domain();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "domain", 
                //    "VisitDomain", "domains", true);
                //this.checkLangs(domain.DOMAIN().GetText(), domain.Start.Line, 
                //    domain.Start.Column, keyLangsList, true);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(domain,
                    "VisitDomain", true);
                this.checkLangs(domain[0].DOMAIN().GetText(), domain[0].Start.Line, 
                    domain[0].Start.Column, keyLangsList, true);
            } // if

            return null;
        } // VisitDomains


        public override object VisitDomain([NotNull] DomainContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "DOMAIN", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.DOMAIN().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            string domainVal = context.multistring().GetText();
            this.checkStringValueLimit(context.DOMAIN().GetText(), context.Start.Line,
                context.Start.Column, domainVal, FpxVisitor.STRING_LIST_VALUE_LIMIT);
            return keyLangPair;
        } // VisitDomain


        public override object VisitVariabletypes([NotNull] VariabletypesContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var vartype = context.variabletype();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "variabletype", 
                //    "VisitVariabletype", "variabletypes", false);
                //this.checkLangs(vartype.VARIABLETYPE().GetText(), vartype.Start.Line, 
                //    vartype.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(vartype,
                    "VisitVariabletype", false);
                this.checkLangs(vartype[0].VARIABLETYPE().GetText(), vartype[0].Start.Line, 
                    vartype[0].Start.Column, keyLangsList, false);
            } // if

            return null;
        } // VisitVariableTypes


        public override object VisitVariabletype([NotNull] VariabletypeContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "VARIABLETYPE", 
                "LANGDECLAR", null);
            this.checkStringValueLimit(context.VARIABLETYPE().GetText(), context.Start.Line, 
                context.Start.Column, context.multistring().GetText(), 
                FpxVisitor.STRING_VALUE_LIMIT);

            return keyLangPair;
        } // VisitVariabletype


        public override object VisitHierarchies([NotNull] HierarchiesContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var hierarchy = context.hierarchy();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "hierarchy", 
                //    "VisitHierarchy", "hierarchies", true);
                //this.checkLangs(hierarchy.HIERARCHIES().GetText(), hierarchy.Start.Line, 
                //    hierarchy.Start.Column, keyLangsList, true);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(hierarchy,
                    "VisitHierarchy", true);
                this.checkLangs(hierarchy[0].HIERARCHIES().GetText(), hierarchy[0].Start.Line, 
                    hierarchy[0].Start.Column, keyLangsList, true);
            } // if

            return null;
        } // VisitHierarchies


        public override object VisitHierarchy([NotNull] HierarchyContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "HIERARCHIES", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.HIERARCHIES().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            this.VisitHierar_vals(context.hierar_vals());

            return keyLangPair;
        } // VisitHierarchy


        public override object VisitHierar_val([NotNull] Hierar_valContext context)
        {
            var parentText = FpxLexer.DefaultVocabulary.GetSymbolicName(FpxLexer.HIERARCHIES);
            this.checkStringValueLimit(parentText, context.Start.Line,
                context.Start.Column, context.str(0).GetText(), 
                FpxVisitor.STRING_VALUE_LIMIT);

            if (context.str(1) != null)
                this.checkStringValueLimit(parentText, context.Start.Line,
                    context.Start.Column, context.str(1).GetText(), 
                    FpxVisitor.STRING_VALUE_LIMIT);

            return base.VisitHierar_val(context);
        } // VisitHierar_val


        public override object VisitHierarchynames([NotNull] HierarchynamesContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var elimin = context.hierarchyname();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "hierarchyname", 
                //    "VisitHierarchyname", "hierarchynames", true);
                //this.checkLangs(elimin.HIERARCHYNAMES().GetText(), elimin.Start.Line, 
                //    elimin.Start.Column, keyLangsList, true);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(elimin,
                    "VisitHierarchyname", true);
                this.checkLangs(elimin[0].HIERARCHYNAMES().GetText(), elimin[0].Start.Line, 
                    elimin[0].Start.Column, keyLangsList, true);
            } // if

            return null;
        } // VisitHierarchynames


        public override object VisitHierarchyname([NotNull] HierarchynameContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "HIERARCHYNAMES", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.HIERARCHYNAMES().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            string[] hnameVals = context.stringlist().GetText().Split(FpxVisitor.COMMA);
            this.checkStringListValueLimit(context.HIERARCHYNAMES().GetText(), 
                context.Start.Line, context.Start.Column, hnameVals, 
                FpxVisitor.STRING_LIST_VALUE_LIMIT);

            return keyLangPair;
        } // VisitHierarchyname


        public override object VisitMaps([NotNull] MapsContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var map = context.map();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "map", 
                //    "VisitMap", "maps", true);
                //this.checkLangs(map.MAP().GetText(), map.Start.Line, 
                //    map.Start.Column, keyLangsList, true);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(map,
                    "VisitMap", true);
                this.checkLangs(map[0].MAP().GetText(), map[0].Start.Line, 
                    map[0].Start.Column, keyLangsList, true);
            } // if

            return null;
        } // VisitMaps


        public override object VisitMap([NotNull] MapContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "MAP", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.MAP().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            string[] mapVals = context.stringlist().GetText().Split(FpxVisitor.COMMA);
            this.checkStringListValueLimit(context.MAP().GetText(), 
                context.Start.Line, context.Start.Column, mapVals, 
                FpxVisitor.STRING_LIST_VALUE_LIMIT);

            return keyLangPair;
        } // VisitMap


        public override object VisitPartitioneds([NotNull] PartitionedsContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var partit = context.partitioned();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "partitioned", 
                //    "VisitPartitioned", "partitioneds", true);
                //this.checkLangs(partit.PARTITIONED().GetText(), partit.Start.Line, 
                //    partit.Start.Column, keyLangsList, true);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(partit,
                    "VisitPartitioned", true);
                this.checkLangs(partit[0].PARTITIONED().GetText(), partit[0].Start.Line, 
                    partit[0].Start.Column, keyLangsList, true);
            } // if

            return null;
        } // VisitPartitioneds


        public override object VisitPartitioned([NotNull] PartitionedContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "PARTITIONED", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.PARTITIONED().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            this.VisitComplex_text(context.complex_text());

            return keyLangPair;
        } // VisitPartitioned


        public override object VisitCompl_val([NotNull] Compl_valContext context)
        {
            var parentText = FpxLexer.DefaultVocabulary.GetSymbolicName(FpxLexer.PARTITIONED);

            if (context.STRINGVAL() != null)
                this.checkStringValueLimit(parentText, context.Start.Line,
                    context.Start.Column, context.STRINGVAL().GetText(), 
                    FpxVisitor.STRING_VALUE_LIMIT);

            return base.VisitCompl_val(context);
        } // VisitCompl_val


        public override object VisitEliminations([NotNull] EliminationsContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var elimin = context.elimination();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "elimination", 
                //    "VisitElimination", "eliminations", true);
                //this.checkLangs(elimin.ELIMINATION().GetText(), elimin.Start.Line, 
                //    elimin.Start.Column, keyLangsList, true);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(elimin,
                    "VisitElimination", true);
                this.checkLangs(elimin[0].ELIMINATION().GetText(), elimin[0].Start.Line, 
                    elimin[0].Start.Column, keyLangsList, true);
            } // if

            return null;
        } // VisitEliminations


        public override object VisitElimination([NotNull] EliminationContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "ELIMINATION", 
                "LANGDECLAR", "DIMDECLAR");

            bool dimsOK = this.checkDimension(context.ELIMINATION().GetText(), context.Start.Line, 
                context.Start.Column, keyLangPair.Value, this.extractDim(keyLangPair.Key));
            if (!dimsOK)
                return default(KeyValuePair<string, string>);

            if (context.elim_val() != null && context.elim_val().multistring() != null)
            {
                string elimVal = context.elim_val().multistring().GetText();
                this.checkStringValueLimit(context.ELIMINATION().GetText(), context.Start.Line,
                    context.Start.Column, elimVal, FpxVisitor.STRING_LIST_VALUE_LIMIT);
            } // if

            return keyLangPair;
        } // VisitElimination


        public override object VisitLastupdated([NotNull] LastupdatedContext context)
        {
            if (context.DATETIMEVAL() != null)
                this.checkStringValueLimit(context.LASTUPDATED().GetText(), context.Start.Line,
                    context.Start.Column, context.DATETIMEVAL().GetText(), 
                    FpxVisitor.STRING_VALUE_LIMIT);
            return base.VisitLastupdated(context);
        } // VisitLastupdated


        public override object VisitSurveys([NotNull] SurveysContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var survey = context.survey();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "survey", 
                //    "VisitSurvey", "surveys", false);
                //this.checkLangs(survey.SURVEY().GetText(), survey.Start.Line, 
                //    survey.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(survey,
                    "VisitSurvey", false);
                this.checkLangs(survey[0].SURVEY().GetText(), survey[0].Start.Line, 
                    survey[0].Start.Column, keyLangsList, false);
            } // if

            return null;
        } // VisitSurveys


        public override object VisitSurvey([NotNull] SurveyContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "SURVEY", 
                "LANGDECLAR", null);
            string linkVal = context.multistring().GetText();
            this.checkStringValueLimit(context.SURVEY().GetText(), context.Start.Line,
                context.Start.Column, linkVal, FpxVisitor.STRING_VALUE_LIMIT);

            return keyLangPair;
        } // VisitSurvey


        public override object VisitLinks([NotNull] LinksContext context)
        {
            if (context.Parent is PxfileContext)
            {
                var link = context.link();
                //var keyLangsList = this.collectLanguagesForKeyword(context, "link", 
                //    "VisitLink", "links", false);
                //this.checkLangs(link.LINK().GetText(), link.Start.Line, 
                //    link.Start.Column, keyLangsList, false);
                var keyLangsList = this.collectLanguagesForKeywordNonRec(link,
                    "VisitLink", false);
                this.checkLangs(link[0].LINK().GetText(), link[0].Start.Line, 
                    link[0].Start.Column, keyLangsList, false);
            } // if

            return null;
        } // VisitLinks


        public override object VisitLink([NotNull] LinkContext context)
        {
            var keyLangPair = this.visitAndExtractLangFromKeyword(context, "LINK", 
                "LANGDECLAR", null);
            string linkVal = context.multistring().GetText();
            this.checkStringValueLimit(context.LINK().GetText(), context.Start.Line,
                context.Start.Column, linkVal, FpxVisitor.STRING_VALUE_LIMIT);

            return keyLangPair;
        }


        public override object VisitFirstpublished([NotNull] FirstpublishedContext context)
        {

            if ((context.yesnodate() != null) && (context.yesnodate().DATETIMEVAL() != null))
            {
                var date = context.yesnodate().DATETIMEVAL().GetText();
                this.checkStringValueLimit(context.FIRSTPUBLISHED().GetText(), context.Start.Line,
                    context.Start.Column, date, FpxVisitor.STRING_VALUE_LIMIT);
            } // if
            return base.VisitFirstpublished(context);
        } // VisitFirstpublished
    } // class FpxVisitor
} // namespace Fpx
