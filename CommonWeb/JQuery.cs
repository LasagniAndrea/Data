using EFS.ACommon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.UI;
using Tz = EFS.TimeZone;
// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    /// <summary>
    /// Description résumée de JQuery.
    /// Classe AutoComplete
    /// Classe UI
    /// Classe Datepicker , DateTimePicker et TimePicker
    /// Classe Toggle
    /// </summary>
    // EG 20210126 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
    // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
    // EG 20240619 [WI945] Security : Update outdated components (JQuery, JQueryUI, BlockUI, Cookie & QTip2)
    // JQuery           : 3.5.1     => 3.7.1
    // JQueryUI         : 1.12.1    => 1.13.1
    // Block to BlockUI : 1.2.3     => 2.7.0
    // Cookie           : 3.0.0     => 3.0.5
    // Qtip to QTip2    : 1.0.0-rc3 => Qtip2 (3.0.3)
    public sealed partial class JQuery
    {
        #region Constants
        /// <summary>
        /// Javascript Source declaration
        /// </summary>
        public const string SCRIPTDECLARATION = @"<script type=""text/javascript"" src=""{0}""></script>";
        /// <summary>
        /// Path to the JQuery engine
        /// </summary>
        public const string JQUERYENGINEPATH = "Javascript/JQuery/jquery-3.7.1.min.js";
        public const string JQUERYMIGRATEPATH = "Javascript/JQuery/jquery-migrate-3.3.2.js";
        /// <summary>
        /// Init Format function instruction
        /// </summary>
        public const string INITFUNCSFORMAT = @"if (typeof($) != ""undefined"") $().ready(function() {{" + "{0}\n" + "}});";
        #endregion Constants
        #region Members
        /// <summary>
        /// Autocomplete engines supported by the Spheres plateform
        /// </summary>
        /// <remarks>
        /// Just one script can be linked for each engine
        /// </remarks>
        public enum Engines
        {
            JQuery,
            //SimplifiedJQuery,
            //YUI,
            //AJAX
        }
        #endregion Members
        #region Constructor
        public JQuery() { }        
		#endregion Constructor
        #region Methods
        #region WriteScript
        /// <summary>
        /// Write the script reference 
        /// </summary>
        /// <param name="pPage">Page</param>
        /// <param name="pKey">Key</param>
        /// <param name="pScript">FullPath Script</param>
        public static void WriteScript(PageBase pPage, string pKey, string pScript)
        {
            pPage.RegisterScript(pKey, String.Format(SCRIPTDECLARATION, pScript),false);
        }
        #endregion WriteScript
        #region WriteEngineScript
        /// <summary>
        /// Write the script engine reference 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pEngine"></param>
        /// EG 20170918 [23342] Add VirtualPath
        /// EG 20210126 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1) Laisser la ligne JQueryMigrate en commentaire
        public static void WriteEngineScript(PageBase pPage, Engines pEngine)
        {
            switch (pEngine)
            {
                case Engines.JQuery:
                    WriteScript(pPage, "JQueryEngine", pPage.VirtualPath(JQUERYENGINEPATH));
                    //WriteScript(pPage, "JQueryMigrate", pPage.VirtualPath(JQUERYMIGRATEPATH));
                    break;
            }
        }
        #endregion WriteEngineScript
        #region WriteFunctionScripts
        // EG 20210222 [XXXXX] Suppression commentaires
        public static void WriteFunctionScripts(PageBase pPage, string pKey, string pFunction)
        {
            StringBuilder sbjss = new StringBuilder();
            sbjss.Append("\n<script type=\"text/javascript\">\n");
            //sbjss.Append("<!--\n");
            sbjss.AppendLine(pFunction);
            //sbjss.Append("\n// -->");
            sbjss.Append("\n</script>");
            pPage.RegisterScript(pKey, sbjss.ToString(),false);
        }
        #endregion WriteFunctionScripts
        #region WriteInitialisationScripts
        // EG 20210222 [XXXXX] Suppression commentaires
        public static void WriteInitialisationScripts(PageBase pPage, string pKey, string pContents)
        {
            StringBuilder sbjss = new StringBuilder();
            sbjss.Append("\n<script type=\"text/javascript\">\n");
            //sbjss.Append("<!--\n");
            sbjss.AppendLine(String.Format(INITFUNCSFORMAT, pContents));
            //sbjss.Append("\n// -->");
            sbjss.Append("\n</script>");
            pPage.RegisterScript(pKey, sbjss.ToString(), true);
        }
        #endregion WriteInitialisationScripts

        #region WriteSelectOptGroupScripts
        //public static void WriteSelectOptGroupScripts(PageBase pPage, string pClientId, Pair<string, string>[] pOptGroup)
        public static void WriteSelectOptGroupScripts(PageBase pPage, string pClientId, List<Pair<string, string>> pOptGroup)
        {
            WriteSelectOptGroupScripts(pPage, pClientId, pOptGroup, true);
        }
        // EG 20150108 Add 1 < pOptGroup.Count
        public static void WriteSelectOptGroupScripts(PageBase pPage, string pClientId, List<Pair<string, string>> pOptGroup, bool pIsWithBkgFunction)
        {
            if (1 < pOptGroup.Count)
            {
                string contents = string.Empty;

                //----------------------------------------------------------------
                //Option Group
                //----------------------------------------------------------------
                //for (int i = 0; i < pOptGroup.Length; i++)
                for (int i = 0; i < pOptGroup.Count; i++)
                {
                    contents += String.Format(@"$(""select#{0} option[optiongroup='{1}']"").wrapAll(""<optgroup label='{2}'>"");",
                                pClientId, pOptGroup[i].First, pOptGroup[i].Second) + Cst.CrLf;
                }
                JQuery.WriteInitialisationScripts(pPage, "optgroup_" + pClientId, contents);

                //----------------------------------------------------------------
                //BackColor on Focus/Blur
                //----------------------------------------------------------------
                if (pIsWithBkgFunction)
                {
                    contents = string.Empty;
                    contents += String.Format(@"$('select#{0}').focus(", pClientId) + @" function() {" + Cst.CrLf;
                    //contents += "alert('focus');" + Cst.CrLf;
                    contents += String.Format(@"$('select#{0}').css('background-color','#A4A4A4');", pClientId) + Cst.CrLf;
                    contents += @"});" + Cst.CrLf;
                    contents += String.Format(@"$('select#{0}').blur(", pClientId) + @" function() {" + Cst.CrLf;
                    //contents += "alert('blur');" + Cst.CrLf;
                    contents += String.Format(@"$('select#{0}').css('background-color','');", pClientId) + Cst.CrLf;
                    contents += @"});" + Cst.CrLf;
                    JQuery.WriteInitialisationScripts(pPage, "focus_" + pClientId, contents);
                }
            }
        }
        #endregion WriteSelectOptGroupScripts

        #endregion Methods



        #region UI
        /// <summary>
        /// Static helper class to JQuery UI initialisation content
        /// </summary>
        // EG 20171025 [23509] Del InitParamDatePicker, InitParamDateTimePicker, InitParamDateTimeOffsetPicker
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210126 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        // EG 20210505 [25700] FreezeGrid implementation
        // EG 20240619 [WI945] Security : Update outdated components (JQueryUI, BlockUI, Cookie & QTip)
        public static partial class UI
        {
            #region Constants
            /// <summary>
            /// Path to the UI plugin based on the JQuery engine
            /// </summary>
            /// EG 20170918 [23342] Change Path
            public const string PLUGIN_TIMEPICKER = "Javascript/JQuery/jquery-ui-timepicker.min.js";
            public const string PLUGIN_DIALOG = "Javascript/JQuery/jquery.dialogWrapper.min.js";
            public const string PLUGIN_QTIP2 = "Javascript/JQuery/jquery.qtip2.min.js";
            public const string PLUGIN_FREEZEGRID = "Javascript/JQuery/jquery.freezegrid.min.js";
            public const string PLUGIN_BLOCK = "Javascript/JQuery/jquery.blockUI-2.70.0.min.js";
            public const string COOKIEPATH = "Javascript/JQuery/jquery.Cookie-3.0.5.min.js";

            public const string CSSLINKID = "linkCssJQueryUI";
            // EG 20170918 [23342] New
            public const string CSSTIMEPICKER = "linkCssTimePicker";
            public const string CSSQTIP2 = "linkCssQTip2";

            public const string PATH = "Javascript/JQuery/jquery-ui-1.13.3.min.js";
            public const string CSSPATH = "Javascript/JQuery/jquery-ui-1.13.3.min.css";
            public const string CULTUREPATH = "Javascript/JQuery/jquery-ui-1.13.3.culture.min.js";
            // EG 20170918 [23342] New
            public const string CSSPATHTIMEPICKER = "Javascript/JQuery/jquery-ui-timepicker.min.css";
            public const string CSSPATHQTIP2 = "Javascript/JQuery/jquery.qtip2.min.css";
            #endregion Constants
            #region Members
            public enum Effect
            {
                blind,
                clip,
                drop,
                explode,
                fade,
                fold,
                puff,
                slide,
                scale,
                size,
                pulsate,
            }
            public enum Speed
            {
                slow,
                fast,
            }

            public enum Duration
            {
                slow,
                normal,
                fast,
            }
            public enum ShowOn
            {
                focus,
                button,
                both,
            }
            public enum ShowAnim
            {
                show,
                slideDown,
                fadeIn,
            }

            // EG 20170918 [23342] New DateTimeOffsetPicker, TimeOffsetPicker
            // EG 20240619 [WI945] Qtip2 instead of QTip
            public enum Plugins
            {
                Toggle,
                DatePicker,
                DateTimePicker,
                DateTimeOffsetPicker,
                TimePicker,
                TimeOffsetPicker,
                Dialog,
                Confirm,
                FixTable,
                QTip2,
                Block,
            }
            #endregion Members
            #region Methods
            #region InitParamBlock
            //EG 20120613 BlockUI New
            public static string InitParamBlock(Block pBlock)
            {
                string contents = "message:messagevalue";
                if (StrFunc.IsFilled(pBlock.Message))
                    contents = contents.Replace("messagevalue", JavaScript.JSString(pBlock.Message));
                if (0 < pBlock.FadeIn)
                    contents += ",fadeIn:" + pBlock.FadeIn.ToString();
                if (0 < pBlock.FadeIn)
                    contents += ",fadeOut:" + pBlock.FadeOut.ToString();
                if (StrFunc.IsFilled(pBlock.Title))
                    contents += ",title:" + JavaScript.JSString(pBlock.Title);
                contents += ",centerX:true";
                contents += ",centerY:true";
                // EG 20130725 Timout paramètrable
                contents += ",timeout:" + pBlock.Timeout.ToString();
                contents += ",ignoreIfBlocked: true"; 
                if (pBlock.Theme)
                {
                    contents += ",theme:true";
                    contents += ",themedCSS:{";
                }
                else
                {
                    contents += ",css:{padding:0,margin:0,top:'40%',left:'35%', textAlign:'center',color:'#000',border:'3px solid #848484',backgroundColor:'#fff',cursor:'wait',";
                }
                contents += "opacity:" + pBlock.Opacity;
                if (StrFunc.IsFilled(pBlock.Width))
                    contents += ",width:'" + pBlock.Width + "'";
                contents += "}";
                //contents += ",overlayCSS:{";
                //contents += "opacity:" + pBlock.Opacity + "}";
                return contents;
            }
            #endregion InitParamBlock
            #region InitParamUnblock
            //EG 20120613 BlockUI New
            public static string InitParamUnblock(Block pBlock)
            {
                string contents = string.Empty;
                if (0 < pBlock.FadeOut)
                    contents += "fadeOut:" + pBlock.FadeOut.ToString();
                return contents;
            }
            #endregion InitParamBlock
            #region InitParamDialog
            // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
            // EG 20210126 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
            public static string InitParamDialog(Dialog pDialog)
            {
                string contents = "closeOnEscape:true";
                contents += ",closeText:'" + pDialog.CloseText + "'";
                contents += ",height:'" + pDialog.Height + "'";
                contents += ",width:'" + pDialog.Width + "'";
                // EG 20210120 [25556] la position est gérée en dure (centrée)
                //contents += ",position:'" + pDialog.Position + "'";
                if (StrFunc.IsFilled(pDialog.DialogClass))
                    contents += ",dialogClass:'" + pDialog.DialogClass + "'";

                if (0 < pDialog.MaxHeight)
                    contents += ",maxHeight:" + pDialog.MaxHeight;
                if (0 < pDialog.MaxWidth)
                    contents += ",maxWidth:" + pDialog.MaxWidth;
                if (pDialog.Modal)
                    contents += ",modal:true";
                if (false == pDialog.Resizable)
                    contents += ",resizable:false";

                contents += ",open:function(){$(this).parents('.ui-dialog-buttonpane button:eq(0)').trigger('focus');}";
                return contents;
            }
            #endregion InitParamDialog
            #region InitParamFixTable
            public static string InitParamFixTable(FixTable pFixTable)
            {
                string contents = "width:" + pFixTable.Width.ToString();
                contents += ",height:" + pFixTable.Height.ToString();
                contents += ",fixedColumns:" + pFixTable.NbFixedColumns.ToString();
                contents += ",classHeader:'" + pFixTable.CssHeader + "'";
                contents += ",classFooter:'" + pFixTable.CssFooter + "'";
                contents += ",classColumn:'" + pFixTable.CssColumn + "'";
                contents += ",fixedColumnWidth:" + pFixTable.FixedColumnsWidth.ToString();
                if (StrFunc.IsFilled(pFixTable.BackColor))
                    contents += ",fixedColumnbackcolor:'" + pFixTable.BackColor + "'";
                if (StrFunc.IsFilled(pFixTable.HoverColor))
                    contents += ",fixedColumnhovercolor:'" + pFixTable.HoverColor + "'";
                return contents;
            }
            #endregion InitParamFixTable
            #region WriteHeaderLink
            /// <summary>
            /// Write the css link reference
            /// </summary>
            /// <param name="header"></param>
            /// <param name="engine"></param>
            public static void WriteHeaderLink(Control pHeader, Engines pEngine)
            {
                WriteHeaderLink(pHeader, pEngine, CSSPATH);
            }
            /// EG 20170918 [23342] Add link CSS for TIMEPICKER, Add VirtualPath
            // EG 20240619 [WI945] New QTip2
            public static void WriteHeaderLink(Control pHeader, Engines pEngine, string pPath)
            {
                switch (pEngine)
                {
                    case Engines.JQuery:
                        string cssLinkID = CSSLINKID;
                        string cssPath = pPath;
                        string cssTimePicker = CSSTIMEPICKER;
                        string cssPathTimePicker = CSSPATHTIMEPICKER;
                        string cssQTip2 = CSSQTIP2;
                        string cssPathQTip2 = CSSPATHQTIP2;

                        if (pHeader.Page is PageBase headerPage)
                        {
                            cssLinkID = headerPage.VirtualPath(cssLinkID);
                            cssPath = headerPage.VirtualPath(cssPath);
                            cssTimePicker = headerPage.VirtualPath(CSSTIMEPICKER);
                            cssPathTimePicker = headerPage.VirtualPath(CSSPATHTIMEPICKER);
                            cssPathQTip2 = headerPage.VirtualPath(CSSPATHQTIP2);
                        }
                        PageTools.SetHeaderLink(pHeader, cssLinkID, cssPath);
                        PageTools.SetHeaderLink(pHeader, cssTimePicker, cssPathTimePicker);
                        PageTools.SetHeaderLink(pHeader, cssQTip2, cssPathQTip2);
                        break;
                }
            }
            #endregion WriteHeaderLink
            #region WritePluginScript
            /// <summary>
            /// Write the script reference of the plugin
            /// </summary>
            /// EG 20170918 [23342] Add Add VirtualPath
            // EG 20210126 [25556] MAJ JQuery (3.5.1) suppression Addon inutilisé
            // EG 20210505 [25700] FreezeGrid implementation 
            // EG 20240619 [WI945] Security : Update outdated components (QTip2)
            public static void WritePluginScript(PageBase pPage, Engines pEngine)
            {
                switch (pEngine)
                {
                    case Engines.JQuery:
                        WriteScript(pPage, "JQueryCookie", pPage.VirtualPath(COOKIEPATH));
                        WriteScript(pPage, "JQueryUI", pPage.VirtualPath(PATH));
                        WriteScript(pPage, "JQueryUICulture", pPage.VirtualPath(CULTUREPATH));
                        WriteScript(pPage, "JQueryTimePickerAddon", pPage.VirtualPath(PLUGIN_TIMEPICKER));
                        WriteScript(pPage, "JQueryDialogAddon", pPage.VirtualPath(PLUGIN_DIALOG));
                        WriteScript(pPage, "JQueryFreezeGrid", pPage.VirtualPath(PLUGIN_FREEZEGRID));
                        WriteScript(pPage, "JQueryQTip2Addon", pPage.VirtualPath(PLUGIN_QTIP2));
                        WriteScript(pPage, "JQueryBlockAddon", pPage.VirtualPath(PLUGIN_BLOCK));
                        break;
                }
            }
            #endregion WritePluginScript
            #region WriteInitialisationScripts (DATEPICKER)
            /// EG 20170918 [23342] Upd
            /// EG 20170926 [22374] Upd
            // EG 20190123 Refactoring String.Format
            // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
            // EG 20210120 [25556] désactivation tabindex sur image calendrier
            public static void WriteInitialisationScripts(PageBase pPage, DatePicker pDatePicker)
            {
                string contents = String.Format(@"function DatePicker() {{
                if (typeof($) != 'undefined') {{
                    $.datepicker.setDefaults($.datepicker.regional['{0}']);
                    $('.DtPicker').datepicker({{{1}, 
                        onClose:function(event,ui) {{ui.input.focus();}}, 
                        onSelect:function() {{this.fireEvent && this.fireEvent('onchange') || $(this).change();}}
                        }});
                    }}
                }}", pDatePicker.Culture, pDatePicker.SetOptions());

                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.DatePicker.ToString(), contents);
                JQuery.WriteInitialisationScripts(pPage, "JQuery" + Plugins.DatePicker.ToString(), "DatePicker();$('.ui-datepicker-trigger').each(function() {{$(this).attr('tabindex', '-1');}});");
            }
            #endregion WriteInitialisationScripts (DATEPICKER)
            #region WriteInitialisationScripts (DATETIMEPICKER)
            /// EG 20170918 [23342] Upd
            /// EG 20170926 [22374] Upd
            // EG 20190123 Refactoring String.Format
            public static void WriteInitialisationScripts(PageBase pPage, DateTimePicker pDateTimePicker)
            {
                string contents = String.Format(@"function DateTimePicker() {{
                if (typeof($) != 'undefined') {{
                    $.datepicker.setDefaults($.datepicker.regional['{0}']);
                    $('.DtTimePicker').datetimepicker({{{1}, 
                        onClose:function(event,ui) {{ui.input.focus();}}, 
                        onSelect:function() {{this.fireEvent && this.fireEvent('onchange') || $(this).change();}}
                        }});
                    }}
                }}", pDateTimePicker.Culture, pDateTimePicker.SetOptions());

                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.DateTimePicker.ToString(), contents);
                JQuery.WriteInitialisationScripts(pPage, "JQuery" + Plugins.DateTimePicker.ToString(), "DateTimePicker();");
            }
            #endregion WriteInitialisationScripts (DATETIMEPICKER)
            #region WriteInitialisationScripts (DATETIMEOFFSETPICKER)
            /// EG 20170918 [23342] New
            /// EG 20170926 [22374] Upd
            // EG 20190123 Refactoring String.Format
            public static void WriteInitialisationScripts(PageBase pPage, DateTimeOffsetPicker pDateTimeOffsetPicker)
            {
                string contents = String.Format(@"function DateTimeOffsetPicker() {{
                if (typeof($) != 'undefined') {{
                    $.datepicker.setDefaults($.datepicker.regional['{0}']);
                    $.timepicker.setDefaults($.timepicker.regional['{0}']);
                    {1}
                    $('.DtTimeOffsetPicker').each(function() {{
                        $(this).datetimepicker({{{2}, 
                            onSelect:function() {{$(this).fireEvent && $(this).fireEvent('onchange') || $(this).change();}}
                            }});
                        }})
                        $('.DtTimeOffsetPickerAlt').each(function() {{
                            $(this).datetimepicker({{{3}, 
                                onSelect:function() {{$(this).fireEvent && $(this).fireEvent('onchange') || $(this).change();}}
                            }});
                        }});
                    }}
                }}", pDateTimeOffsetPicker.Culture, pDateTimeOffsetPicker.SetTimeDefaults(), pDateTimeOffsetPicker.SetOptions(false), pDateTimeOffsetPicker.SetOptions(true));

                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.DateTimeOffsetPicker.ToString(), contents);
                JQuery.WriteInitialisationScripts(pPage, "JQuery" + Plugins.DateTimeOffsetPicker.ToString(), "DateTimeOffsetPicker();");
            }
            #endregion WriteInitialisationScripts (DATETIMEOFFSETPICKER)
            #region WriteInitialisationScripts (FIXEDTABLE)
            // EG 20190123 Refactoring String.Format
            public static void WriteInitialisationScripts(PageBase pPage, FixTable pFixTable)
            {
                string contents = String.Format(@"if (typeof($) != 'undefined') {{
                $().ready(function() {{
                    $('.tableDiv').each(function() {{
                        var id = $(this).get(0).id;
                        $('#' + id + ' .FixedTables').fixedTable({{{0}, outerId:id}});
                        }});
                    }});
                }}", InitParamFixTable(pFixTable));
                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.FixTable.ToString(), contents);
            }
            #endregion WriteInitialisationScripts (FIXEDTABLE)
            #region WriteInitialisationScripts (TIMEPICKER)
            /// EG 20170918 [23342] Upd
            /// EG 20170926 [22374] Upd
            // EG 20190123 Refactoring String.Format
            public static void WriteInitialisationScripts(PageBase pPage, TimePicker pTimePicker)
            {
                string contents = String.Format(@"function TimePicker() {{
                if (typeof($) != 'undefined') {{
                    $.timepicker.setDefaults($.timepicker.regional['{0}']);
                    $('.TimePicker').timepicker({{{1}, 
                        onClose:function(event,ui) {{ui.input.focus();}}, 
                        onSelect:function() {{this.fireEvent && this.fireEvent('onchange') || $(this).change();}}
                        }});
                    }}
                }}", pTimePicker.Culture, pTimePicker.SetOptions());

                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.TimePicker.ToString(), contents);
                JQuery.WriteInitialisationScripts(pPage, "JQuery" + Plugins.TimePicker.ToString(), "TimePicker();");
            }
            #endregion WriteInitialisationScripts (TIMEPICKER)
            #region WriteInitialisationScripts (TIMEOFFSETPICKER)
            /// EG 20170918 [23342] New
            /// EG 20170926 [22374] Upd
            // EG 20190123 Refactoring String.Format
            // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
            // EG 20210120 [25556] désactivation tabindex sur image calendrier
            public static void WriteInitialisationScripts(PageBase pPage, TimeOffsetPicker pTimeOffsetPicker)
            {
                _ = pTimeOffsetPicker.SetOptions(false);

                string contents = String.Format(@"function TimeOffsetPicker() {{
                if (typeof($) != 'undefined') {{
                    $.timepicker.setDefaults($.timepicker.regional['{0}']);
                    $('.TimeOffsetPicker').timepicker({{{1}, 
                        onClose:function(event,ui) {{ui.input.focus();}}, 
                        onSelect:function() {{this.fireEvent && this.fireEvent('onchange') || $(this).change();}}
                        }});
                    }}
                }}", pTimeOffsetPicker.Culture, pTimeOffsetPicker.SetOptions(false));

                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.TimeOffsetPicker.ToString(), contents);
                JQuery.WriteInitialisationScripts(pPage, "JQuery" + Plugins.TimeOffsetPicker.ToString(), "TimeOffsetPicker();$('.ui-datepicker-trigger').each(function() {{$(this).attr('tabindex', '-1');}});");
            }
            #endregion WriteInitialisationScripts (TIMEOFFSETPICKER)
            #region WriteInitialisationScripts (TOGGLE)
            // EG 20190123 Refactoring String.Format
            // EG 20210126 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
            // EG 20211217 [XXXXX] Ajout d'une image matérialisant l'état (ouverture/fermeture) d'un panel avec en-tête, Maintien de cet état avant un post de la page.[XXXXX] Ajout d'une image matérialisant l'état (ouverture/fermeture) d'un panel avec en-tête,
            // EG 20211217 [XXXXX] Maintien de cet état avant un post de la page.
            public static void WriteInitialisationScripts(PageBase pPage, Toggle pToggle)
            {
                string contents = String.Format(@"function Toggle() {{
                if (typeof($) != 'undefined') {{
                    $(""div[class*='headh']"").on(""click"", function(event) {{
                        var oType = 'SELECT|INPUT|LABEL|IMG|OPTION';
                        if ((this == event.target) || (oType.indexOf(event.target.tagName.toUpperCase())==-1)){{
                            var isClosed = ($(this).next().css('display') == 'block');
                            $(this).next().stop(true,true).toggle('{0}','','{1}');
                            $(this).toggleClass('closed', isClosed);

                            if( typeof(SaveToggleStatus) != 'undefined' )
                                SaveToggleStatus();
                            }}
                        }});
                    }};
                    if( typeof(SetToggleStatus) != 'undefined' )
                        SetToggleStatus();
                }};", pToggle.Effect.ToString(), pToggle.Speed.ToString());

                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.Toggle.ToString(), contents);
                JQuery.WriteInitialisationScripts(pPage, "JQuery" + Plugins.Toggle.ToString(), "Toggle();");
            }
            #endregion WriteInitialisationScripts (TOGGLE)
            #region WriteInitialisationScripts (DIALOG)
            // EG 20190123 Refactoring String.Format + Correction Erreur JQuery|Javascript
            // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
            // EG 20201029 [XXXXX] Correction Pas d'affichage Message de réponse d'un traitement terminé
            // EG 20210126 [25556] MAJ JQuery (3.5.1) et JQuery-UI (1.12.1)
            // EG 20211217 [XXXXX] Ajout de Console.Log pour debug
            public static void WriteInitialisationScripts(PageBase pPage, OpenDialog pOpenDialog)
            {
                string dialog = String.Format(@"OpenMainDialog(""{0}"",""{1}"",'{2}','{3}','{4}','{5}','{6}');",
                    pOpenDialog.Title, pOpenDialog.Message, 
                    pOpenDialog.CssDialog, pOpenDialog.Height, pOpenDialog.MaxHeight, pOpenDialog.Width, pOpenDialog.MaxWidth);

                //parent.document.getElementById('main');

                string contents = String.Format(@"function OpenDialog() {{
                    if (typeof($) != 'undefined') {{
                        var mainPage = $('#main')[0];
                        if (null == mainPage)
                            mainPage = parent.document.getElementById('main');
                        if (null != mainPage) {{
                            var mainWindow = mainPage.contentWindow;
                            if ((null != mainWindow) && ('complete' == mainWindow.document.readyState))
                                mainWindow.onload = function() {{
                                    if( typeof(mainWindow.OpenMainDialog) == 'undefined' )
                                        console.log('function mainWindow.OpenMainDialog NOT FOUND');
                                    else
                                        mainWindow.{0}
                                }};
                            else if (null != mainWindow)        
                                mainWindow.document.onreadystatechange = function() {{
                                    if ('complete' === mainWindow.document.readyState)
                                        if( typeof(mainWindow.OpenMainDialog) === 'undefined' )
                                            console.log('function mainWindow.OpenMainDialog NOT FOUND (COMPLETE)');
                                        else
                                            mainWindow.onload = function() {{
                                                if( typeof(mainWindow.OpenMainDialog) === 'undefined' )
                                                    console.log('function mainWindow.OpenMainDialog NOT FOUND');
                                                else
                                                    mainWindow.{0}
                                            }};
                                }}
                        }}
                    }}
                    else 
                    {{
                        console.log('typeof($) is undefined');
                        {0}
                    }}
                }}", dialog);
                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.Dialog.ToString(), contents.Replace("#dialog#", dialog));
                JQuery.WriteInitialisationScripts(pPage, "JQuery" + Plugins.Dialog.ToString(), "OpenDialog();");
            }
            // EG 20190123 Refactoring String.Format + Correction Erreur JQuery|Javascript
            // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
            // EG 20210120 [25556] la valeur par défaut de l'attribut position est utilisé
            public static void WriteInitialisationScripts(PageBase pPage, Dialog pDialog)
            {
                string contents = String.Format(@"function OpenMainDialog(title, message, css, height, maxHeight, width, maxWidth, isClose) {{
                    if (typeof($) != 'undefined') {{
                        var id = $.showDialog(title,message, {{ {0}, buttons:{{ {1} : function() {{
                                $.hideDialog(); 
                                if (isClose=='true') self.close();
                                }}
                            }}
                        }});                       
                        var curDialog = $('#m' + id);
                        curDialog.prev().removeClass('ui-dialog-titlebar-error ui-dialog-titlebar-info i-dialog-titlebar-none ui-dialog-titlebar-warning').addClass('ui-dialog-titlebar-' + css);
                        if (0 < height)
                            curDialog.css('height',height);
                        if (0 < width)
                            curDialog.css('width',width);
                        if (0 < maxHeight)
                            curDialog.css('maxHeight',maxHeight);
                        if (0 < maxWidth)
                            curDialog.css('maxWidth',maxWidth);
                    }}
                }}", InitParamDialog(pDialog),pDialog.CloseText);
                JQuery.WriteFunctionScripts(pPage, "FuncMain_" + Plugins.Dialog.ToString(), contents);
            }
            // EG 20190123 Refactoring String.Format + Correction Erreur JQuery|Javascript
            // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
            // EG 20210120 [25556] la valeur par défaut de l'attribut position est utilisé
            public static void WriteInitialisationScripts(PageBase pPage, Confirm pConfirm)
            {
                string paramConfirm = InitParamDialog(pConfirm);
                string message = JavaScript.HTMLBlockUIMessage(pPage, Ressource.GetString("Msg_WaitingRefresh"));

                string contents = String.Format(@"function OpenMainConfirm(title,message,css,height,maxHeight,width,maxWidth,target, argument_Ok, argument_Cancel) {{
                    if (typeof($) != 'undefined') {{
                        var id = $.showDialog(title, message, {{
                        {0}, buttons:[
                            {{text:'{1}',  id:'btndlg_yes',click: function() {{
                                $.hideDialog();
                                if ((null != target) && (null != argument_Ok)) Block({2});
                                    __doPostBack(target,argument_Ok);
                                }}
                            }},
                            {{text:'{3}',id:'btndlg_no',click: function() {{
                                $.hideDialog();
                                if ((null != target) && (null != argument_Cancel))
                                    __doPostBack(target,argument_Cancel);
                                }}
                            }}]
                        }});

                        var curDialog = $('#m' + id);
                        curDialog.prev().removeClass('ui-dialog-titlebar-error ui-dialog-titlebar-info ui-dialog-titlebar-none ui-dialog-titlebar-warning').addClass('ui-dialog-titlebar-' + css);
                        if (0 < height)
                            curDialog.css('height',height);
                        if (0 < width)
                            curDialog.css('width',width);
                        if (0 < maxHeight)
                            curDialog.css('maxHeight',maxHeight);
                        if (0 < maxWidth)
                            curDialog.css('maxWidth',maxWidth);
                    }}
                }}", paramConfirm, pConfirm.YesText, message, pConfirm.NoText);

                JQuery.WriteFunctionScripts(pPage, "FuncMain_" + Plugins.Confirm.ToString(), contents);

                contents = String.Format(@"function OpenConfirm(title,message,css,height,maxHeight,width,maxWidth, contents_Ok, contents_Cancel) {{
                    if (typeof($) != ""undefined"") {{
                        var id = $.showDialog(title, message, {{
                            {0}, buttons:[{{text:'{1}', id:'btndlg_yes',click: function() {{
                                $.hideDialog(); Block({2}); contents_Ok
                                }}
                            }}, {{text:'{3}', id:'btndlg_no',click: function() {{
                                $.hideDialog(); contents_Cancel
                                }}
                            }}]
                        }});

                        var curDialog = $('#m' + id);
                        curDialog.prev().removeClass('ui-dialog-titlebar-error ui-dialog-titlebar-info ui-dialog-titlebar-none ui-dialog-titlebar-warning').addClass('ui-dialog-titlebar-' + css);
                        if (0 < height)
                            curDialog.css('height',height);
                        if (0 < width)
                            curDialog.css('width',width);
                        if (0 < maxHeight)
                            curDialog.css('maxHeight',maxHeight);
                        if (0 < maxWidth)
                            curDialog.css('maxWidth',maxWidth);
                    }}
                }}", paramConfirm, pConfirm.YesText, message, pConfirm.NoText);

                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.Confirm.ToString(), contents);
            }

            // EG 20190123 Refactoring String.Format
            // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
            // EG 20210120 [25556] la valeur par défaut de l'attribut position est utilisé
            public static void WriteInitialisationScripts(PageBase pPage, AddActor pAddActor)
            {
                string contents = String.Format(@"function ddlSelect_Change() {{
                    var ddlSelectedValue=$('#ddlSelect').val();
                    if (ddlSelectedValue == '0') {{
                        $('#txtActor').val('');
                        $('#txtActor').attr('disabled',true);
                        $('#lblActor').attr('disabled',true);
                        $('#txtBook').val('');
                        $('#txtBook').attr('disabled',true);
                        $('#lblBook').attr('disabled',true);
                    }}
                    else {{
                        $('#txtActor').removeAttr('disabled');
                        $('#lblActor').removeAttr('disabled');
                        $('#txtBook').removeAttr('disabled');
                        $('#lblBook').removeAttr('disabled');
                    }}
                }};

                function Open_Repository(url,pkValue,actorValue,bookValue) {{
                    if (pkValue.length == 0)
                        url += '&NAV='+actorValue;
                    else
                        url += '&DPC=1&PK=IDA&PKV='+pkValue+'&NAV='+actorValue+'&NBV='+bookValue;
                    window.open(url);
                }};

                function OpenAddActorConfirm(title,message,css,height,maxHeight,width,maxWidth,target,argument_Ok,argument_Cancel) {{
                    if (typeof($) != 'undefined') {{
                        var id = $.showDialog(title,message,
                        {{{0}, buttons:[
                            {{text:'{1}',id:'btndlg_yes',click: function() {{
                                    var ddlSelectedValue=$('#ddlSelect').val();
                                    var txtActorValue=$('#txtActor').val();
                                    var txtBookValue=$('#txtBook').val();
                                    if (ddlSelectedValue == '0') {{
                                        ddlSelectedValue='';
                                        txtActorValue='';
                                        txtBookValue='';
                                    }}
                                    $.hideDialog();
                                    if ((null != target) && (null != argument_Ok)) {{Open_Repository(target,ddlSelectedValue,txtActorValue,txtBookValue);}}
                                }}
                            }},
                            {{text:'{2}',id:'btndlg_no',click: function() {{
                                $.hideDialog();
                                if ((null != target) && (null != argument_Cancel)) {{__doPostBack(target,argument_Cancel);}}
                                }}
                            }}]
                        }});

                        var curDialog = $('#m' + id);
                        curDialog.prev().removeClass('ui-dialog-titlebar-error ui-dialog-titlebar-info ui-dialog-titlebar-none ui-dialog-titlebar-warning').addClass('ui-dialog-titlebar-' + css);
                        if (0 < height)
                            curDialog.css('height',height);
                        if (0 < width)
                            curDialog.css('width',width);
                        if (0 < maxHeight)
                            curDialog.css('maxHeight',maxHeight);
                        if (0 < maxWidth)
                            curDialog.css('maxWidth',maxWidth);
                    }}
                }};", InitParamDialog(pAddActor), pAddActor.ContinueText, pAddActor.CancelText);

                JQuery.WriteFunctionScripts(pPage, "FuncAddActor_" + Plugins.Confirm.ToString(), contents);

            }

            // EG 20190123 Refactoring String.Format
            // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
            // EG 20210120 [25556] la valeur par défaut de l'attribut position est utilisé
            public static void WriteInitialisationScripts(PageBase pPage, ActorTransfer pActorTransfer)
            {
                string paramTransfer = InitParamDialog(pActorTransfer);
                string continueText = pActorTransfer.ContinueText;
                string cancelText = pActorTransfer.CancelText;
                string msgWaiting = JavaScript.HTMLBlockUIMessage(pPage, Ressource.GetString("Msg_WaitingProcess"));

                string contents = @"function OpenActorTransferConfirm(title,message,css,height,maxHeight,width,maxWidth,target,argument_Ok,argument_Cancel) {
                    if (typeof($) != 'undefined') {
                        var id = $.showDialog(title,message, {#paramTransfer#, buttons:[
                        {text:'#continueText#',id:'btndlg_yes',click: function() {
                            var txtDealerValue=$('#txtDealer').val();
                            var txtBookDealerValue=$('#txtBookDealer').val();
                            var txtClearerValue=$('#txtClearer').val();
                            var txtBookClearerValue=$('#txtBookClearer').val();
                            var chkReversalFeesValue=$('#chkReversalFees').is(':checked');
                            var chkReversalSafekeepingValue=$('#chkReversalSafekeeping').is(':checked');
                            var chkCalcNewFeesValue=$('#chkCalcNewFees').is(':checked');
                            $.hideDialog();
                            if ((null != target) && (null != argument_Ok)) {
                                Block(#msgWaiting#);
                                argument_Ok = argument_Ok + ';Args;DEALER=' + txtDealerValue + '|BOOKDEALER=' + txtBookDealerValue + '|CLEARER=' + txtClearerValue + '|BOOKCLEARER=' + txtBookClearerValue + '|ISREVERSALFEES=' + chkReversalFeesValue + '|ISCALCNEWFEES=' + chkCalcNewFeesValue + '|ISREVERSALSAFEKEEPING=' + chkReversalSafekeepingValue;
                                __doPostBack(target,argument_Ok);
                            }
                         }},
                         {text:'#cancelText#',id:'btndlg_no',click: function() {
                            $.hideDialog();
                            if ((null != target) && (null != argument_Cancel)) {__doPostBack(target,argument_Cancel);}
                            }
                         }]
                        });

                        var curDialog = $('#m' + id);
                        curDialog.prev().removeClass('ui-dialog-titlebar-error ui-dialog-titlebar-info ui-dialog-titlebar-none ui-dialog-titlebar-warning').addClass('ui-dialog-titlebar-' + css);
                        if (0 < height)
                            curDialog.css('height',height);
                        if (0 < width)
                            curDialog.css('width',width);
                        if (0 < maxHeight)
                            curDialog.css('maxHeight',maxHeight);
                        if (0 < maxWidth)
                            curDialog.css('maxWidth',maxWidth);
                    }
                };";

                JQuery.WriteFunctionScripts(pPage, "FuncActorTransfer_" + Plugins.Confirm.ToString(),
                contents.Replace("#paramTransfer#", paramTransfer).Replace("#msgWaiting#", msgWaiting).Replace("#continueText#", continueText).Replace("#cancelText#", cancelText));
            }

            // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
            // EG 20210120 [25556] la valeur par défaut de l'attribut position est utilisé
            public static void WriteInitialisationScripts2(PageBase pPage, Dialog pDialog)
            {
                string contents = String.Format(@"
                    function OpenDialogAndReload(title, message, css, height, maxHeight, width, maxWidth, target, argument)
                    {{
                        if (typeof($) != ""undefined"") 
                        {{
                            var id = $.showDialog(title,message,
                                        {{
                                            closeOnEscape:true,
                                            closeText:'{0}',
                                            height:'{1}',
                                            width:'{2}',
                                            {3}
                                            {4}
                                            {5}
                                            {6}
                                            open:function()
                                            {{
                                                $(this).parents('.ui-dialog-buttonpane button:eq(0)').focus();
                                            }},
                                            buttons:
                                            {{
                                                '{0}': function() 
                                                {{
                                                    $.hideDialog(); if ((null != target) && (null != argument))  __doPostBack(target,argument);
                                                }}
                                            }}
                                        }});
                            
                            var curDialog = $('#m' + id);

                            curDialog.prev().removeClass('ui-dialog-titlebar-error ui-dialog-titlebar-info ui-dialog-titlebar-none ui-dialog-titlebar-warning').addClass('ui-dialog-titlebar-' + css);

                            if (0 < height)
                                curDialog.css('height',height);
                            if (0 < width)
                                curDialog.css('width',width);
                            if (0 < maxHeight)
                                curDialog.css('maxHeight',maxHeight);
                            if (0 < maxWidth)
                                curDialog.css('maxWidth',maxWidth);
                        }}
                    }};
                ",
                 pDialog.CloseText, 
                 pDialog.Height,
                 pDialog.Width,
                 pDialog.MaxHeight > 0 ? String.Format("maxHeight:{0},", pDialog.MaxHeight) : "",
                 pDialog.MaxWidth > 0 ? String.Format("maxWidth:{0},", pDialog.MaxWidth) : "",
                 pDialog.Modal ? "modal:true," : "",
                 !pDialog.Resizable ? "resizable:false," : "");

                JQuery.WriteFunctionScripts(pPage, "Func_OpenDialogAndReload", contents);
            }
            #endregion WriteInitialisationScripts (DIALOG)

            #region WriteInitialisationScripts (BLOCK)
            //EG 20120613 BlockUI New
            // EG 20190123 Refactoring String.Format
            public static void WriteInitialisationScripts(PageBase pPage, Block pBlock)
            {
                string contents = String.Format(@"function Block(messagevalue) {{
                if (typeof($) != 'undefined') {{
                        $.blockUI.defaults.css = {{}};
                        $.blockUI({{{0}}});
                    }}
                }};
                function Unblock() {{
                    if (typeof($) != 'undefined') {{
                        $.unblockUI({{{1}}});
                    }}
                }};", InitParamBlock(pBlock), InitParamUnblock(pBlock));

                JQuery.WriteFunctionScripts(pPage, "Func_" + Plugins.Block.ToString(), contents);
            }
            #endregion WriteInitialisationScripts (BLOCK)
            #region WriteInitialisationScripts (QTIP2)
            // EG 20240619 [WI945] Security : Update outdated components (QTip2)
            public static void WriteInitialisationScripts(PageBase pPage, QTip2 pQTip)
            {
                JQuery.WriteFunctionScripts(pPage, "FuncMain_" + Plugins.QTip2.ToString(), pQTip.GetFunction());
                JQuery.WriteInitialisationScripts(pPage, "JQuery" + Plugins.QTip2.ToString(), "GlobalQTip2();");
            }
            #endregion WriteInitialisationScripts (QTIP2)
            // EG 20210505 [25700] FreezeGrid implementation 
            public static void WriteInitialisationFreezeGrid(PageBase pPage)
            {
                JQuery.WriteInitialisationScripts(pPage, "JQueryFreeze", "$('#divDtg').freezegrid();");
            }
            #endregion Methods
        }
        #endregion UI

        #region Block
        /// <summary>
        /// helper class to dump the common Block javaScript initialisation content
        /// </summary>
        //EG 20120613 BlockUI New
        public class Block
        {
            #region Members
            private string _width;
            private string _title;
            private string _message;
            private bool _theme;
            private int _timeout;
            #endregion Members
            #region Accessors
            #region Width
            public string Width
            {
                get { return _width; }
                set { _width = value; }
            }
            #endregion Width
            #region Opacity
            public string Opacity
            {
                get { return "0.6"; }
            }
            #endregion Opacity
            #region Title
            public string Title
            {
                get { return _title; }
                set { _title = value; }
            }
            #endregion Title
            #region Message
            public string Message
            {
                get { return _message; }
                set { _message = value; }
            }
            #endregion Message
            #region Theme
            public bool Theme
            {
                get { return _theme; }
                set { _theme = value; }
            }
            #endregion Theme
            #region FadeIn
            public int FadeIn
            {
                get { return 200; }
            }
            #endregion FadeIn
            #region FadeOut
            public int FadeOut
            {
                get { return 200; }
            }
            #endregion FadeOut
            #region Timeout
            public int Timeout
            {
                get { return _timeout; }
                set { _timeout = value; }
            }
            #endregion Timeout
            #endregion Accessors
            #region Constructor
            public Block(PageBase pPage)
            {
                Title = pPage.PageTitle;
                Theme = true;
            }
            public Block(PageBase pPage, string pResMessage)
            {
                Title = pPage.PageTitle;
                Message = Ressource.GetString(pResMessage);
                Theme = true;
            }
            public Block(string pResTitle, string pResMessage, bool pIsUseTheme)
            {
                if (StrFunc.IsFilled(pResTitle))
                    Title = Ressource.GetString(pResTitle);
                if (StrFunc.IsFilled(pResMessage))
                    Message = Ressource.GetString(pResMessage);
                Theme = pIsUseTheme;
            }
            #endregion Constructor
        }
        #endregion Block

        #region DatePicker
        /// <summary>
        /// helper class to dump the common DatePicker javaScript initialisation content
        /// </summary>
        /// EG 20170918 [23342] Add dateFormat
        /// EG 20170926 [22374] Upd 
        public class DatePicker
        {
            #region Members
            private bool _autosize;
            private string _buttonImage;
            private bool _buttonImageOnly;
            private string _buttonText;
            private bool _changeMonth;
            private bool _changeYear;
            private bool _constrainInput;
            private string _culture;
            private Nullable<JQuery.UI.Duration> _duration;
            private bool _gotoCurrent;
            private bool _hideIfNoPrevNext;
            private Nullable<DateTime> _minDate;
            private Nullable<DateTime> _maxDate;
            private bool _showButtonPanel;
            private Nullable<JQuery.UI.ShowOn> _showOn;
            private Nullable<JQuery.UI.ShowAnim> _showAnim;
            private bool _showOtherMonths;
            private string _yearRange;
            private string _dateFormat;
            #endregion Members
            #region Accessors
            #region Autosize
            public bool Autosize
            {
                get { return _autosize; }
                set { _autosize = value; }
            }
            #endregion Autosize
            #region ButtonImage
            public string ButtonImage
            {
                get { return _buttonImage; }
                set { _buttonImage = value; }
            }
            #endregion ButtonImage
            #region ButtonImageOnly
            public bool ButtonImageOnly
            {
                get { return _buttonImageOnly; }
                set { _buttonImageOnly = value; }
            }
            #endregion ButtonImageOnly
            #region ButtonText
            public string ButtonText
            {
                get { return _buttonText; }
                set { _buttonText = value; }
            }
            #endregion ButtonText
            #region ChangeMonth
            public bool ChangeMonth
            {
                get { return _changeMonth; }
                set { _changeMonth = value; }
            }
            #endregion ChangeMonth
            #region ChangeYear
            public bool ChangeYear
            {
                get { return _changeYear; }
                set { _changeYear = value; }
            }
            #endregion ChangeYear
            #region ConstrainInput
            public bool ConstrainInput
            {
                get { return _constrainInput; }
                set { _constrainInput = value; }
            }
            #endregion ConstrainInput
            #region Culture
            public string Culture
            {
                get { return _culture; }
                set { _culture = value; }
            }
            #endregion Culture
            #region DateFormat
            // EG 20170918 [23342] New
            public string DateFormat
            {
                get { return _dateFormat; }
                set { _dateFormat = value.Replace("yy", "y").ToLower(); }
            }
            #endregion DateFormat
            #region Duration
            public Nullable<JQuery.UI.Duration> Duration
            {
                get { return _duration; }
                set { _duration = value; }
            }
            #endregion ButtonText
            #region GotoCurrent
            public bool GotoCurrent
            {
                get { return _gotoCurrent; }
                set { _gotoCurrent = value; }
            }
            #endregion GotoCurrent
            #region HideIfNoPrevNext
            public bool HideIfNoPrevNext
            {
                get { return _hideIfNoPrevNext; }
                set { _hideIfNoPrevNext = value; }
            }
            #endregion HideIfNoPrevNext
            #region MinDate
            public Nullable<DateTime> MinDate
            {
                get { return _minDate; }
                set { _minDate = value; }
            }
            #endregion MinDate
            #region MaxDate
            public Nullable<DateTime> MaxDate
            {
                get { return _maxDate; }
                set { _maxDate = value; }
            }
            #endregion MaxDate            
            #region ShowAnim
            public Nullable<JQuery.UI.ShowAnim> ShowAnim
            {
                get { return _showAnim; }
                set { _showAnim = value; }
            }
            #endregion ShowAnim
            #region ShowButtonPanel
            public bool ShowButtonPanel
            {
                get { return _showButtonPanel; }
                set { _showButtonPanel = value; }
            }
            #endregion ShowButtonPanel
            #region ShowOn
            public Nullable<JQuery.UI.ShowOn> ShowOn
            {
                get { return _showOn; }
                set { _showOn = value; }
            }
            #endregion ShowOn
            #region ShowOtherMonths
            public bool ShowOtherMonths
            {
                get { return _showOtherMonths; }
                set { _showOtherMonths = value; }
            }
            #endregion ShowOtherMonths
            #region YearRange
            public string YearRange
            {
                get { return _yearRange; }
                set { _yearRange = value; }
            }
            #endregion YearRange
            #endregion Accessors
            #region Constructor
            /// EG 20170918 [23342] Upd
            /// EG 20170918 [23452] Upd
            public DatePicker() : this(UI.ShowOn.button) { }
            // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Gestion du bouton Calendar
            public DatePicker(UI.ShowOn pShowOn)
            {
                Autosize = true;
                ButtonImageOnly = false;
                ChangeMonth = true;
                ChangeYear = true;
                ConstrainInput = false;
                Culture = Thread.CurrentThread.CurrentUICulture.Name;
                DateFormat = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern;
                Duration = JQuery.UI.Duration.fast;
                GotoCurrent = true;
                HideIfNoPrevNext = true;
                ShowOtherMonths = true;
                ShowAnim = JQuery.UI.ShowAnim.slideDown;
                ShowOn = pShowOn;
                Validator validatorDate = Validator.GetValidatorDateRange(string.Empty,string.Empty);
                MinDate = new DtFunc().StringToDateTime(validatorDate.MinValue, DtFunc.FmtShortDate).Date;
                MaxDate = new DtFunc().StringToDateTime(validatorDate.MaxValue, DtFunc.FmtShortDate).Date;
                YearRange = MinDate.Value.Year.ToString() + ":" + MaxDate.Value.Year.ToString();
            }
            #endregion Constructor
            #region Methods
            #region SetOptions
            /// EG 20170926 [22374] New
            // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Gestion du bouton Calendar
            // EG 20240619 [WI945] Security : Update outdated components (Date Picker without XML tag on button)
            public virtual string SetOptions()
            {
                string _options = "dateFormat:'" + DateFormat + "'";
                //_options += @",buttonText:""<i class='fa-icon fas fa-calendar-alt'></i>""";
                _options += @",buttonText:''";
                if (Duration.HasValue)
                    _options += ",duration:'" + Duration.Value.ToString() + "'";
                if (ShowOn.HasValue)
                    _options += ",showOn:'" + ShowOn.Value.ToString() + "'";
                if (ShowAnim.HasValue)
                    _options += ",showAnim:'" + ShowAnim.Value.ToString() + "'";
                if (StrFunc.IsFilled(YearRange))
                    _options += ",yearRange:'" + YearRange + "'";

                if (false == ConstrainInput)
                    _options += ",constrainInput:false";

                if (Autosize)
                    _options += ",autosize:false";
                if (ButtonImageOnly)
                    _options += ",buttonImageOnly:true";
                if (ChangeMonth)
                    _options += ",changeMonth:true";
                if (ChangeYear)
                    _options += ",changeYear:true";
                if (GotoCurrent)
                    _options += ",gotoCurrent:true";
                if (HideIfNoPrevNext)
                    _options += ",hideIfNoPrevNext:true";
                if (ShowButtonPanel)
                    _options += ",showButtonPanel:true";
                if (ShowOtherMonths)
                    _options += ",showOtherMonths:true";
                return _options;
            }
            #endregion SetOptions
            #endregion Methods
        }
        #endregion DatePicker
        #region DateTimePicker
        /// <summary>
        /// helper class to dump the common DateTimePicker javaScript initialisation content
        /// </summary>
        /// EG 20170918 [23342] Upd showHour, showMinute, showSecond
        /// EG 20170918 [23342] New pickerTimeFormat, timeInput
        /// EG 20170926 [22374] Upd
        /// EG 20170918 [23452] Add timeControlType, onLine
        public class DateTimePicker : DatePicker
        {
            #region Members
            private string _timeControlType;
            private Nullable<bool> _oneLine;
            private Nullable<int> _hour;
            private Nullable<int> _minute;
            private Nullable<int> _second;
            private Nullable<bool> _showHour;
            private Nullable<bool> _showMinute;
            private Nullable<bool> _showSecond;
            private Nullable<int> _stepHour;
            private Nullable<int> _stepMinute;
            private Nullable<int> _stepSecond;
            private string _timeFormat;
            private string _pickerTimeFormat;
            private bool _timeInput;
            #endregion Members
            #region Accessors
            #region TimeControlType
            public string TimeControlType
            {
                get { return _timeControlType; }
                set { _timeControlType = value; }
            }
            #endregion TimeControlType
            #region Hour
            public Nullable<int> Hour
            {
                get { return _hour; }
                set { _hour = value; }
            }
            #endregion Hour
            #region Minute
            public Nullable<int> Minute
            {
                get { return _minute; }
                set { _minute = value; }
            }
            #endregion Minute
            #region OneLine
            public Nullable<bool> OneLine
            {
                get { return _oneLine; }
                set { _oneLine = value; }
            }
            #endregion OneLine
            #region PickerTimeFormat
            // EG 20170918 [23342] New
            public string PickerTimeFormat
            {
                get { return _pickerTimeFormat; }
                set { _pickerTimeFormat = value; }
            }
            #endregion PickerTimeFormat
            #region Second
            public Nullable<int> Second
            {
                get { return _second; }
                set { _second = value; }
            }
            #endregion Second
            #region ShowHour
            // EG 20170918 [23342] Upd
            public Nullable<bool> ShowHour
            {
                get { return _showHour; }
                set { _showHour = value; }
            }
            #endregion ShowHour
            #region ShowMinute
            // EG 20170918 [23342] Upd
            public Nullable<bool> ShowMinute
            {
                get { return _showMinute; }
                set { _showMinute = value; }
            }
            #endregion ShowMinute
            #region ShowSecond
            // EG 20170918 [23342] Upd
            public Nullable<bool> ShowSecond
            {
                get { return _showSecond; }
                set { _showSecond = value; }
            }
            #endregion ShowSecond
            #region StepHour
            public Nullable<int> StepHour
            {
                get { return _stepHour; }
                set { _stepHour = value; }
            }
            #endregion StepHour
            #region StepMinute
            public Nullable<int> StepMinute
            {
                get { return _stepMinute; }
                set { _stepMinute = value; }
            }
            #endregion StepMinute
            #region StepSecond
            public Nullable<int> StepSecond
            {
                get { return _stepSecond; }
                set { _stepSecond = value; }
            }
            #endregion StepSecond
            #region TimeFormat
            public string TimeFormat
            {
                get { return _timeFormat; }
                set { _timeFormat = value; }
            }
            #endregion TimeFormat
            #region TimeInput
            // EG 20170918 [23342]
            public bool TimeInput
            {
                get { return _timeInput; }
                set { _timeInput = value; }
            }
            #endregion TimeInput
            #endregion Accessors
            #region Constructor
            public DateTimePicker() : this(UI.ShowOn.button) { }
            // EG 20170918 [23342]
            public DateTimePicker(UI.ShowOn pShowOn) : base(pShowOn)
            {
                TimeInput = true;
                TimeFormat = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.LongTimePattern.Replace("tt","TT");
                ShowHour = false;
                ShowMinute = false;
                ShowSecond = false;
            }
            #endregion Constructor
            #region Methods
            #region SetOptions
            /// EG 20170926 [22374] New
            /// EG 20170918 [23452] Add timeControlType, onLine 
            public override string SetOptions()
            {
                string _options = base.SetOptions();
                _options += ",timeFormat:'" + TimeFormat + "'";

                if (StrFunc.IsFilled(TimeControlType))
                    _options += ",controlType:'" + TimeControlType + "'";

                if (OneLine.HasValue)
                    _options += ",oneLine:" + (BoolFunc.IsTrue(OneLine.Value) ? "true" : "false");

                if (ShowHour.HasValue)
                    _options += ",showHour:" + (BoolFunc.IsTrue(ShowHour.Value) ? "true" : "false");
                if (ShowMinute.HasValue)
                    _options += ",showMinute:" + (BoolFunc.IsTrue(ShowMinute.Value) ? "true" : "false");
                if (ShowSecond.HasValue)
                    _options += ",showSecond:" + (BoolFunc.IsTrue(ShowSecond.Value) ? "true" : "false");


                _options += ",alwaysSetTime:true";

                if (TimeInput)
                    _options += ",timeInput:true";

                if (StepHour.HasValue && (StepHour.Value > 1))
                    _options += ",stepHour:" + StepHour.Value;
                if (StepMinute.HasValue && (StepMinute.Value > 1))
                    _options += ",stepMinute:" + StepMinute.Value;
                if (StepSecond.HasValue && (StepSecond.Value > 1))
                    _options += ",stepSecond:" + StepSecond.Value;

                if (Hour.HasValue && (Hour.Value > 0))
                    _options += ",hour:" + Hour.Value;
                if (Minute.HasValue && (Minute.Value > 0))
                    _options += ",minute:" + Minute.Value;
                if (Second.HasValue && (Second.Value > 0))
                    _options += ",second:" + Second.Value;

                return _options;
            }
            #endregion SetOptions
            #endregion Methods
        }
        #endregion DateTimePicker
        #region DateTimeOffsetPicker
        /// <summary>
        /// helper class to dump the common DateTimePicker javaScript initialisation content
        /// </summary>
        /// EG 20170822 [23342] New
        /// EG 20170926 [22374] Upd
        public class DateTimeOffsetPicker : DateTimePicker
        {
            #region Members
            private Nullable<long> _milliSecond;
            private Nullable<long> _microSecond;
            private Nullable<bool> _showMilliSecond;
            private Nullable<bool> _showMicroSecond;
            private Nullable<bool> _showTimezone;
            private Nullable<int> _stepMilliSecond;
            private Nullable<int> _stepMicroSecond;
            private string _timeParse;
            private bool _isTimeZoneList;
            private Nullable<bool> _altFieldTimeOnly;
            private Nullable<bool> _altRedirectFocus;
            private Nullable<bool> _alwaysSetTime;
            private string _altTimeFormat;
            #endregion Members
            #region Accessors
            #region MilliSecond
            public Nullable<long> MilliSecond
            {
                get { return _milliSecond; }
                set { _milliSecond = value; }
            }
            #endregion MilliSecond
            #region MicroSecond
            public Nullable<long> MicroSecond
            {
                get { return _microSecond; }
                set { _microSecond = value; }
            }
            #endregion MicroSecond

            #region ShowMilliSecond
            public Nullable<bool> ShowMilliSecond
            {
                get { return _showMilliSecond; }
                set { _showMilliSecond = value; }
            }
            #endregion ShowMilliSecond
            #region ShowMicroSecond
            public Nullable<bool> ShowMicroSecond
            {
                get { return _showMicroSecond; }
                set { _showMicroSecond = value; }
            }
            #endregion ShowMicroSecond
            #region ShowTimezone
            public Nullable<bool> ShowTimezone
            {
                get { return _showTimezone; }
                set { _showTimezone = value; }
            }
            #endregion ShowTimezone
            #region StepMilliSecond
            public Nullable<int> StepMilliSecond
            {
                get { return _stepMilliSecond; }
                set { _stepMilliSecond = value; }
            }
            #endregion StepMilliSecond
            #region StepMicroSecond
            public Nullable<int> StepMicroSecond
            {
                get { return _stepMicroSecond; }
                set { _stepMicroSecond = value; }
            }
            #endregion StepMicroSecond

            #region TimeParse
            public string TimeParse
            {
                get { return _timeParse; }
                set { _timeParse = value; }
            }
            #endregion TimeParse

            #region IsTimeZoneList
            public bool IsTimeZoneList
            {
                get { return _isTimeZoneList; }
                set { _isTimeZoneList = value; }
            }
            #endregion IsTimeZoneList
            
            #region AltFieldTimeOnly
            public Nullable<bool> AltFieldTimeOnly
            {
                get { return _altFieldTimeOnly; }
                set { _altFieldTimeOnly = value; }
            }
            #endregion AltFieldTimeOnly
            #region AltRedirectFocus
            public Nullable<bool> AltRedirectFocus
            {
                get { return _altRedirectFocus; }
                set { _altRedirectFocus = value; }
            }
            #endregion AltRedirectFocus
            #region AltTimeFormat
            public string AltTimeFormat
            {
                get { return _altTimeFormat; }
                set { _altTimeFormat = value; }
            }
            #endregion AltTimeFormat
            #region AlwaysSetTime
            public Nullable<bool> AlwaysSetTime
            {
                get { return _alwaysSetTime; }
                set { _alwaysSetTime = value; }
            }
            #endregion AlwaysSetTime

            #endregion Accessors
            #region Constructor
            public DateTimeOffsetPicker() : this(UI.ShowOn.button) { }
            public DateTimeOffsetPicker(UI.ShowOn pShowOn)
                : base(pShowOn)
            {
                // transformation du format Micro/Millisecondes .net pour timePicker (.ffffff => .lc)
                DateTimeFormatInfo dfi = DtFunc.DateTimeOffsetPattern;
                TimeFormat = Regex.Replace(dfi.LongTimePattern, ".ffffff zzz", ".lc").Replace("tt", "TT");
                ShowMicroSecond = false;
                ShowMilliSecond = false;
                ShowTimezone = false;
                //TimeParse = "loose";

                IsTimeZoneList = false;
            }
            #endregion Constructor
            #region Methods
            #region SetTimeDefaults
            /// EG 20170926 [22374] New
            public string SetTimeDefaults()
            {
                string _defaults = string.Empty;
                if (StrFunc.IsFilled(TimeParse))
                    _defaults += "parse:'" + TimeParse + "'";

                if (StrFunc.IsFilled(_defaults))
                    _defaults = String.Format(" $.timepicker.setDefaults({0}{1}{2});","{", _defaults, "}");
                return _defaults;
            }
            #endregion SetTimeDefaults
            #region SetOptions
            /// EG 20170926 [22374] New
            public string SetOptions(bool pIsWithAltField)
            {
                // EG 20171004 Temporaire car si pas d'ouverture sur le Focus (Time est remis à zéro !!!)
                //ShowOn = pIsWithAltField?UI.ShowOn.focus:UI.ShowOn.button;

                string _options = SetOptions();
                if (StrFunc.IsFilled(PickerTimeFormat))
                    _options += ",pickerTimeFormat:'" + PickerTimeFormat + "'";
                if (ShowMilliSecond.HasValue)
                    _options += ",showMillisec:" + (BoolFunc.IsTrue(ShowMilliSecond.Value) ? "true" : "false");
                if (ShowMicroSecond.HasValue)
                    _options += ",showMicrosec:" + (BoolFunc.IsTrue(ShowMicroSecond.Value) ? "true" : "false");
                if (ShowTimezone.HasValue)
                    _options += ",showTimezone:" + (BoolFunc.IsTrue(ShowTimezone.Value) ? "true" : "false");
                if (AlwaysSetTime.HasValue)
                    _options += ",alwaysSetTime:" + (BoolFunc.IsTrue(ShowSecond.Value) ? "true" : "false");

                if (IsTimeZoneList)
                    _options += ",timezoneList:[ " + Tz.Tools.TimezonePickerList + "]";

                if (pIsWithAltField)
                {
                    _options += ",altField: '#' + this.id + '_Time'";

                    if (AltFieldTimeOnly.HasValue)
                        _options += ",altFieldTimeOnly:" + (BoolFunc.IsTrue(AltFieldTimeOnly.Value) ? "true" : "false");
                    if (AltRedirectFocus.HasValue)
                        _options += ",altRedirectFocus:" + (BoolFunc.IsTrue(AltRedirectFocus.Value) ? "true" : "false");
                    if (StrFunc.IsFilled(AltTimeFormat))
                        _options += ",altTimeFormat:'" + AltTimeFormat + "'";
                }
                return _options;
            }
            #endregion SetOptions
            #endregion Methods

        }
        #endregion DateTimeOffsetPicker
        #region FixTable
        /// <summary>
        /// helper class to dump the common FixedTable javaScript initialisation content
        /// </summary>
        public class FixTable
        {
            #region Members
            private int _width;
            private int _height;
            private int _nbFixedColumns;
            private string _cssHeader;
            private string _cssColumn;
            private string _cssFooter;
            private int _fixedColumnsWidth;
            private string _backColor;
            private string _hoverColor;
            #endregion Members
            #region Accessors
            #region Width
            public int Width
            {
                get { return _width; }
                set { _width = value; }
            }
            #endregion Width
            #region Height
            public int Height
            {
                get { return _height; }
                set { _height = value; }
            }
            #endregion Height
            #region NbFixedColumns
            public int NbFixedColumns
            {
                get { return _nbFixedColumns; }
                set { _nbFixedColumns = value; }
            }
            #endregion NbFixedColumns
            #region CssHeader
            public string CssHeader
            {
                get { return _cssHeader; }
                set { _cssHeader = value; }
            }
            #endregion CssHeader
            #region CssColumn
            public string CssColumn
            {
                get { return _cssColumn; }
                set { _cssColumn = value; }
            }
            #endregion CssColumn
            #region CssFooter
            public string CssFooter
            {
                get { return _cssFooter; }
                set { _cssFooter = value; }
            }
            #endregion CssFooter
            #region FixedColumnsWidth
            public int FixedColumnsWidth
            {
                get { return _fixedColumnsWidth; }
                set { _fixedColumnsWidth = value; }
            }
            #endregion FixedColumnsWidth
            #region BackColor
            public string BackColor
            {
                get { return _backColor; }
                set { _backColor = value; }
            }
            #endregion BackColor
            #region HoverColor
            public string HoverColor
            {
                get { return _hoverColor; }
                set { _hoverColor = value; }
            }
            #endregion HoverColor
            #endregion Accessors
            #region Constructor
            public FixTable()
            {
                NbFixedColumns = 6;
                Width = 600;
                Height = 500;
                CssHeader = "fixedHead";
                CssColumn = "fixedColumn";
                CssFooter = "fixedFoot";
                FixedColumnsWidth = 350;
                //BackColor = "#8B569F";
                //HoverColor = "#9E75B0";
            }
            #endregion Constructor
            #region Methods
            #endregion Methods
        }
        #endregion FixTable

        #region QTip2
        /// <summary>
        /// New version of tooltip management. Qtip2 replaces Qtip.
        /// This version is also no longer maintained today.
        /// </summary>
        /// EG 20240619 [WI945] Security : Update outdated components (New QTip2)        
        public class QTip2
        {
            #region Accessors
            public string PositionDef
            {
                get { return @"my: 'center left', at: 'right center', adjust: { method: 'flip shift' }"; }
            }
            public string PositionClassic
            {
                get { return @"my: 'top center', at: 'bottom center', adjust: { method: 'flip shift' }"; }
            }
            public string Position
            {
                get { return @"my: 'center left', at: 'right center', adjust: { method: 'flip shift' }, viewport:$('#divbody')"; }
            }
            public string Hide
            {
                get { return @"delay:0, fixed:true, target:false, event:'click', effect: {type:'slide', length:100}"; }
            }
            public string Show
            {
                get { return @"delay:140, solo:true, ready:false, target:false, event:'click', effect: {type:'fade', length:100}"; }
            }
        #endregion Accessors
        #region Constructor
        public QTip2()
            {
            }
            #endregion Constructor
            #region Methods
            private string QtipWithButton
            {
                get
                {
                    return $@"
                    tt_button = $(this).attr('qtip-button');
                    $(this).qtip({{
                        content:{{attr:'qtip-alt',text:tt_content,title:{{text:tt_title, button:tt_button}}}},
                        position:{{{Position}}}, 
                        show:{{{Show}}}, 
                        hide:{{{Hide}}}, 
                        style:{{classes:tt_style}}
                    }});";
                }
            }
            private string QtipClassic
            {
                get
                {
                    return $@"$(this).qtip(
                    {{content:{{attr:'qtip-alt',text:tt_content,title:{{text:tt_title}}}}, 
                    position:{{{PositionClassic}}},
                    style:{{classes:tt_style}}
                    }});";
                }
            }
            private string QtipDefault
            {
                get
                {
                    return $@"$(this).qtip(
                    {{content:{{attr:'qtip-alt',text:tt_content}},
                    position:{{{PositionDef}}},
                    style:{{classes:tt_style}}
                    }});";
                }
            }

            public string GetFunction()
            {
                string contents = $@"function GlobalQTip2() {{
                    if (typeof($) != 'undefined') {{
                        $('*[qtip-alt]').each(function() {{

                            var tt_content = $(this).attr('qtip-alt');
                            var tt_title = false;
                            var tt_style = 'qtip-def';

                            if ($(this).attr('qtip-style'))     
                                tt_style = $(this).attr('qtip-style');

                            var tt_button = false;

                            if ($(this).attr('title'))     
                                tt_title = $(this).attr('title');

                            if ($(this).attr('qtip-button'))
                            {{
                                tt_button = true;
                                {{{QtipWithButton}}}
                            }}
                            else if (tt_style == 'qtip-def')
                                {{{QtipDefault}}}

                            else
                                {{{QtipClassic}}}

                            this.removeAttribute('title');
                            this.removeAttribute('qtip-alt');
                            this.removeAttribute('qtip-style');
                            this.removeAttribute('qtip-button');
                           
                        }});
                    }}
                }}";
                return contents;
            }
            #endregion Methods
        }
        #endregion QTip2
        #region TimePicker
        /// <summary>
        /// helper class to dump the common TimePicker javaScript initialisation content
        /// </summary>
        /// EG 20170918 [23452] Add timeControlType, onLine
        public class TimePicker : DateTimePicker
        {
            #region Constructor
            public TimePicker() : this(UI.ShowOn.button) { }
            public TimePicker(UI.ShowOn pShowOn)
                : base(pShowOn)
            {
                ButtonText = Ressource.GetString("TimeEvents");
                TimeInput = false;
                ShowHour = true;
                ShowMinute = true;
                ShowSecond = true;
                TimeControlType = "select";
                OneLine = true;
            }
            #endregion Constructor
            #region Methods
            #endregion Methods
        }
        #endregion TimePicker
        #region TimeOffsetPicker
        /// EG 20170918 [23342] New
        public class TimeOffsetPicker : DateTimeOffsetPicker
        {
            #region Constructor
            public TimeOffsetPicker() : this(UI.ShowOn.button) { }
            public TimeOffsetPicker(UI.ShowOn pShowOn) : base(pShowOn){}
            #endregion Constructor
        }
        #endregion TimeOffsetPicker

        #region Toggle
        /// <summary>
        /// helper class to dump the common Toggle javaScript initialisation content
        /// </summary>
        public class Toggle
        {
            #region Members
            private JQuery.UI.Effect _effect;
            private JQuery.UI.Speed _speed;
            #endregion Members
            #region Accessors
            #region Effect
            public JQuery.UI.Effect Effect
            {
                get { return _effect; }
                set { _effect = value; }
            }
            #endregion Effect
            #region Speed
            public JQuery.UI.Speed Speed
            {
                get { return _speed; }
                set { _speed = value; }
            }
            #endregion Speed
            #endregion Accessors
            #region Constructor
            public Toggle()
            {
                Effect = UI.Effect.fade;
                Speed = UI.Speed.fast;
            }
            #endregion Constructor
            #region Methods
            #endregion Methods
        }
        #endregion Toggle

        #region Dialog
        /// <summary>
        /// helper class to dump the common Dialog javaScript initialisation content
        /// </summary>
        public class Dialog
        {
            #region Members
            protected bool _disabled;
            protected bool _autoOpen;
            protected string _buttons;
            protected bool _closeOnEscape;
            protected string _closeText;
            protected string _dialogClass;
            protected bool _draggable;
            protected string _height;
            protected string _hide;
            protected int _maxHeight;
            protected int _maxWidth;
            protected int _minHeight;
            protected int _minWidth;
            protected bool _modal;
            protected string _position;
            protected bool _resizable;
            protected string _show;
            protected bool _stack;
            protected string _title;
            protected string _width;
            protected int _zindex;
            #endregion Members
            #region Accessors
            #region AutoOpen
            public bool AutoOpen
            {
                get { return _autoOpen; }
                set { _autoOpen = value; }
            }
            #endregion AutoOpen
            #region CloseText
            public string CloseText
            {
                get { return _closeText; }
                set { _closeText = value; }
            }
            #endregion CloseText
            #region DialogClass
            public string DialogClass
            {
                get { return _dialogClass; }
                set { _dialogClass = value; }
            }
            #endregion DialogClass
            #region Disabled
            public bool Disabled
            {
                get { return _disabled; }
                set { _disabled = value; }
            }
            #endregion Disabled
            #region Height
            public string Height
            {
                get { return _height; }
                set { _height = value; }
            }
            #endregion Height
            #region MaxHeight
            public int MaxHeight
            {
                get { return _maxHeight; }
                set { _maxHeight = value; }
            }
            #endregion MaxHeight
            #region MaxWidth
            public int MaxWidth
            {
                get { return _maxWidth; }
                set { _maxWidth = value; }
            }
            #endregion MaxWidth
            #region Modal
            public bool Modal
            {
                get { return _modal; }
                set { _modal = value; }
            }
            #endregion Modal
            #region Position
            public string Position
            {
                get { return _position; }
                set { _position = value; }
            }
            #endregion Position
            #region Resizable
            public bool Resizable
            {
                get { return _resizable; }
                set { _resizable = value; }
            }
            #endregion Resizable
            #region Title
            public string Title
            {
                get { return _title; }
                set { _title = value; }
            }
            #endregion Title
            #region Width
            public string Width
            {
                get { return _width; }
                set { _width = value; }
            }
            #endregion Width

            #endregion Accessors
            #region Constructor
            public Dialog()
            {
                SetDefault();
            }
            public Dialog(string pTitle)
            {
                _title = pTitle;
                SetDefault();
            }
            #endregion Constructor
            #region Methods
            #region SetDefault
            private void SetDefault()
            {
                _modal = true;
                _position = "center";
                _closeText = Ressource.GetString("btnClose");
                _maxWidth = 600;
                _maxHeight = 400;
                _width = "auto";
                _height = "auto";
            }
            #endregion SetDefault
            #endregion Methods
        }
        #endregion Dialog
        #region OpenDialog
        public class OpenDialog : Dialog
        {
            #region Members
            private readonly ProcessStateTools.StatusEnum _status;
            private string _message;
            #endregion Members
            #region Accessors
            #region Message
            public string Message
            {
                get { return _message; }
                set { _message = value; }
            }
            #endregion Message
            #region CssDialog
            public string CssDialog
            {
                get { return _status.ToString().ToLower(); }
            }
            #endregion CssDialog
            #endregion Accessors
            #region Constructor
            public OpenDialog(string pTitle, string pMessage, ProcessStateTools.StatusEnum pStatus)
                : base(pTitle)
            {
                // RD 20130628 Remplacer "\r\n" par "<br/>"
                //_message = pMessage.Replace(Cst.Tab, String.Empty).Replace(Cst.CrLf, Cst.HTMLBreakLine).Replace("\"", "'");
                _message = pMessage.Replace(Cst.Tab, String.Empty).Replace(Cst.CrLf, String.Empty).Replace("\"", "'");
                _status = pStatus;
            }
            #endregion Constructor
        }
        #endregion OpenDialog
        #region DialogMessage
        public class DialogMessage
        {
            #region Members
            private readonly ProcessStateTools.StatusEnum m_Status;
            private readonly string m_Message;
            private readonly string m_Link;
            #endregion Members
            #region Accessors
            #region Level
            public ProcessStateTools.StatusEnum Status
            {
                get { return m_Status; }
            }
            #endregion Level
            #region Message
            public string Message
            {
                get { return m_Message; }
            }
            #endregion Message
            #region Link
            public string Link
            {
                get { return m_Link; }
            }
            #endregion Link
            #endregion Accessors
            #region Constructors
            public DialogMessage()
            {
            }
            public DialogMessage(ProcessStateTools.StatusEnum pStatus, string pMessage, string pLink)
            {
                m_Status = pStatus;
                m_Message = pMessage;
                m_Link = pLink;
            }
            #endregion Constructors


        }
        #endregion DialogMessage

        #region OpenConfirm
        public class OpenConfirm : Confirm
        {
            #region Members
            private readonly ProcessStateTools.StatusEnum _status;
            private string _message;
            #endregion Members
            #region Accessors
            #region Message
            public string Message
            {
                get { return _message; }
                set { _message = value; }
            }
            #endregion Message
            #region CssDialog
            public string CssDialog
            {
                get { return _status.ToString().ToLower(); }
            }
            #endregion CssDialog
            #endregion Accessors
            #region Constructor
            public OpenConfirm(string pTitle, string pMessage, ProcessStateTools.StatusEnum pStatus)
                : base(pTitle)
            {
                _message = pMessage.Replace(Cst.Tab, String.Empty).Replace(Cst.CrLf, String.Empty).Replace("\"", "'");
                _status = pStatus;
            }
            #endregion Constructor
        }
        #endregion OpenConfirm
        #region Confirm
        public class Confirm : Dialog
        {
            #region Members
            protected string _yesText;
            protected string _noText;
            #endregion Members
            #region Accessors
            #region YesText
            public string YesText
            {
                get { return _yesText; }
                set { _yesText = value; }
            }
            #endregion YesText
            #region NoText
            public string NoText
            {
                get { return _noText; }
                set { _noText = value; }
            }
            #endregion NoText
            #endregion Accessors
            #region Constructor
            public Confirm():base()
            {
                SetDefault();
            }
            public Confirm(string pTitle):base(pTitle)
            {
                SetDefault();
            }
            #endregion Constructor
            #region Methods
            #region SetDefault
            private void SetDefault()
            {
                _noText = Ressource.GetString("btnNo");
                _yesText = Ressource.GetString("btnYes");
            }
            #endregion SetDefault
            #endregion Methods
        }
        #endregion Confirm
        #region AddActor
        public class AddActor : Dialog
        {
            #region Members
            protected string _continueText;
            protected string _cancelText;
            #endregion Members
            #region Accessors
            #region ContinueText
            public string ContinueText
            {
                get { return _continueText; }
                set { _continueText = value; }
            }
            #endregion ContinueText
            #region CancelText
            public string CancelText
            {
                get { return _cancelText; }
                set { _cancelText = value; }
            }
            #endregion CancelText
            #endregion Accessors
            #region Constructor
            public AddActor()
                : base()
            {
                SetDefault();
            }
            public AddActor(string pTitle)
                : base(pTitle)
            {
                SetDefault();
            }
            #endregion Constructor
            #region Methods
            #region SetDefault
            private void SetDefault()
            {
                _cancelText = Ressource.GetString("btnCancel");
                _continueText = Ressource.GetString("btnContinue");
                _dialogClass = "addActor";
            }
            #endregion SetDefault
            #endregion Methods
        }
        #endregion AddActor    
        #region ActorTransfer
        public class ActorTransfer : Dialog
        {
            #region Members
            protected string _continueText;
            protected string _cancelText;
            #endregion Members
            #region Accessors
            #region ContinueText
            public string ContinueText
            {
                get { return _continueText; }
                set { _continueText = value; }
            }
            #endregion ContinueText
            #region CancelText
            public string CancelText
            {
                get { return _cancelText; }
                set { _cancelText = value; }
            }
            #endregion CancelText
            #endregion Accessors
            #region Constructor
            public ActorTransfer()
                : base()
            {
                SetDefault();
            }
            public ActorTransfer(string pTitle)
                : base(pTitle)
            {
                SetDefault();
            }
            #endregion Constructor
            #region Methods
            #region SetDefault
            private void SetDefault()
            {
                _cancelText = Ressource.GetString("btnCancel");
                _continueText = Ressource.GetString("btnProcess");
                _dialogClass = "actortransfer";
            }
            #endregion SetDefault
            #endregion Methods
        }
        #endregion ActorTransfer    
    }
}
