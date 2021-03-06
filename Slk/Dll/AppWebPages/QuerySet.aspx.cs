/* Copyright (c) Microsoft Corporation. All rights reserved. */

// QuerySet.aspx.cs
//
// Renders the contents of the query set frame (left pane) of ALWP.
//
// URL example:
//
// ALWP/QuerySet.aspx?QuerySet=LearnerQuerySet
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Schema;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.SharePointLearningKit.Schema;
using Microsoft.SharePointLearningKit;
using Resources.Properties;
using Microsoft.SharePointLearningKit.WebControls;
using Microsoft.SharePointLearningKit.WebParts;
using System.Data.SqlClient;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    /// <summary>
    /// Code Behind Class For QuerySet.aspx.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Alwp")]
    public partial class AlwpQuerySet : QueryBasePage
    {

        /// <summary>See <see cref="SlkAppBasePage.OverrideMasterPage"/>.</summary>
        protected override bool OverrideMasterPage
        {
            get { return false ;}
        }

        #region Page_Load
        private void AddCoreCss(HtmlTextWriter writer, int lcid)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
            string cssUrl = String.Format(CultureInfo.InvariantCulture, "/_layouts/{0}/styles/core.css", lcid);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, cssUrl);
            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 1, writer);
        }

        private void RenderHead(HtmlTextWriter hw)
        {
            // render the "<head>" element and its contents
            using (new HtmlBlock(HtmlTextWriterTag.Head, 1, hw))
            {
                // create a link to "core.css"; 

                // "/_layouts/1033/styles/core.css" except with "1033" replaced with the
                // current SPWeb language code
                // Include 1033 one as a back up
                AddCoreCss(hw, 1033);
                SlkCulture culture = new SlkCulture(SPWeb);
                AddCoreCss(hw, culture.Culture.LCID);

#if SP2007
                //Adds the Theme Css Url to Enable Theming in the frame.
                if (!string.IsNullOrEmpty(SPWeb.ThemeCssUrl))
                {
                    hw.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                    hw.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                    hw.AddAttribute(HtmlTextWriterAttribute.Href, SPWeb.ThemeCssUrl);
                    HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 0, hw);
                }
#endif

                // create a link to ALWP's "Styles.css"
                hw.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                hw.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                hw.AddAttribute(HtmlTextWriterAttribute.Href, "Include/Styles.css");
                HtmlBlock.WriteFullTag(HtmlTextWriterTag.Link, 0, hw);

                // write a "<script>" element that loads "QuerySet.js"
                hw.AddAttribute(HtmlTextWriterAttribute.Src, "Include/QuerySet.js");
                HtmlBlock.WriteFullTag(HtmlTextWriterTag.Script, 0, hw);
            }

        }

        private void RenderQuerySummary(HtmlTextWriter hw, QuerySetDefinition querySetDef)
        {
            using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
            {
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                {
                    // write a hidden iframe that loads QuerySummary.aspx which, 
                    // when loaded, will
                    // call the SetQueryCounts() JScript method in QuerySet.js to update the
                    // query counts that are initially displayed as hourglasses
                    hw.AddAttribute(HtmlTextWriterAttribute.Width, "1");
                    hw.AddAttribute(HtmlTextWriterAttribute.Height, "1");
                    hw.AddAttribute("frameborder", "0");

                    hw.Write("<iframe width=\"1\" height=\"1\" frameborder=\"0\" id=\"{0}QS\" name=\"{0}QS\"></iframe>", FrameId);
                    WriteSummaryForm(hw);
                    WriteResultsForm(hw);

                    // write script code that begins loading the query counts,
                    // and provides other information to QuerySet.js

                    using (new HtmlBlock(HtmlTextWriterTag.Script, 1, hw))
                    {
                        // Register SelectQuery Client Click JavaScript Method.

                        RegisterQuerySetClientScriptBlock(hw);
                        hw.WriteLine();
                        // tell QuerySet.js the names of the queries
                        hw.WriteEncodedText("var QueryNames = new Array();");
                        hw.WriteLine();
                        int queryIndex = 0;
                        int selectedQueryIndex = 0;
                        foreach (QueryDefinition queryDef in querySetDef.Queries)
                        {
                            hw.Write(String.Format(CultureInfo.InvariantCulture, "QueryNames[{0}] = \"{1}\";", queryIndex, queryDef.Name));
                            hw.WriteLine();
                            if (queryDef.Name == querySetDef.DefaultQueryName)
                            {
                                selectedQueryIndex = queryIndex;
                            }

                            queryIndex++;
                        }

                        string scopeScript = @"
                            var scope = parent.scope{0};
                            var queryInput = document.getElementById('alwpSPWebScopeQR');
                            queryInput.value = scope;
                            queryInput = document.getElementById('alwpSPWebScopeQSForm');
                            queryInput.value = scope;
                        ";

                        scopeScript = string.Format(scopeScript, FrameId);
                        hw.WriteLine(scopeScript);

                        // tell QuerySet.js to start loading the selected query results
                        hw.WriteEncodedText(String.Format(CultureInfo.InvariantCulture, "SelectQuery({0});", selectedQueryIndex));


                        hw.WriteLine(@"
                    var summaryForm = document.getElementById('{0}QSForm');
                    summaryForm.submit();", FrameId);
                    }
                }
            }
        }

        private void RenderQueryLines(HtmlTextWriter hw, QuerySetDefinition querySetDef)
        {
            hw.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Empty);
            using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
            {
                hw.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Empty);
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                {

                    hw.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                    hw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    hw.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-toolbar");
                    hw.AddAttribute(HtmlTextWriterAttribute.Style, "border: none; background-image: none; width:100%; height:100%;");
                    hw.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Empty);
                    using (new HtmlBlock(HtmlTextWriterTag.Table, 1, hw))
                    {
                        // write "<col>" tags:
                        // column 1: spacer
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Col, 1, hw);
                        // column 2: left pseudo-border
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Col, 1, hw);
                        // column 3: label
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Col, 1, hw);
                        // column 4: count
                        hw.AddAttribute(HtmlTextWriterAttribute.Style, "text-align: right;");
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Col, 1, hw);

                        // write a spacer row
                        hw.AddAttribute(HtmlTextWriterAttribute.Height, "8");
                        using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
                        {
                            hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                            {
                                HtmlBlock.WriteBlankGif("8", "1", hw);
                            }

                            hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);
                            hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
                            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);
                            hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "border-right: solid 1px #E0E0E0; ");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                            {
                                HtmlBlock.WriteBlankGif("1", "1", hw);
                            }
                        }

                        // write the query link rows
                        int queryIndex = 0;
                        foreach (QueryDefinition queryDef in querySetDef.Queries)
                        {
                            RenderQueryLinkRow(queryDef, queryIndex, hw);
                            queryIndex++;
                        }

                        // write a row that consumes the remaining space 
                        hw.AddAttribute(HtmlTextWriterAttribute.Style, "height:100%;");
                        using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
                        {
                            hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                                HtmlBlock.WriteBlankGif("8", "1", hw);
                            hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);
                            hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
                            HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);
                            hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style,
                                    "border-right: solid 1px #E0E0E0; ");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                                HtmlBlock.WriteBlankGif("1", "1", hw);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Page Load for AlwpQuerySet. 
        /// </summary> 
        /// <param name="sender">an object referencing the source of the event</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // render the HTML for the page
                using (HtmlTextWriter hw = new HtmlTextWriter(Response.Output, "  "))
                {
                    // render the "<html>" element and its contents
                    using (new HtmlBlock(HtmlTextWriterTag.Html, 0, hw))
                    {
                        RenderHead(hw);
                        try
                        {
                            string querySetName = QueryString.ParseString(QueryStringKeys.QuerySet);

                            // set <querySetDef> to the QuerySetDefinition named <querySetName>
                            QuerySetDefinition querySetDef = SlkStore.Settings.FindQuerySetDefinition(querySetName, true);
                            if (querySetDef == null)
                            {
                                throw new SafeToDisplayException (PageCulture.Resources.AlwpQuerySetNotFound, querySetName);
                            }

                            // render the "<body>" element and its contents
                            //hw.AddAttribute(HtmlTextWriterAttribute.Style, "overflow: hidden;");
                            hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 100%; overflow-y: auto;");
                            hw.AddAttribute(HtmlTextWriterAttribute.Id, "SlkAlwpQuerySet");
                            hw.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Empty);
                            using (new HtmlBlock(HtmlTextWriterTag.Body, 0, hw))
                            {
                                AssignmentListWebPart.DumpCultures(hw, "Page_Load");
                                // begin the query set
                                hw.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                                hw.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                                hw.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                                hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-toolbar");
                                hw.AddAttribute(HtmlTextWriterAttribute.Style, "border: none; background-image: none; width:100%; height:100%;");
                                hw.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Empty);
                                using (new HtmlBlock(HtmlTextWriterTag.Table, 1, hw))
                                {
                                    RenderQueryLines(hw, querySetDef);
                                    RenderQuerySummary(hw, querySetDef);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SlkError slkError;                        
                            //Handles SqlException separately to capture the deadlock 
                            //and treat it differently
                            SqlException sqlEx = ex as SqlException;
                            if (sqlEx != null)
                            {                           
                                slkError = WebParts.ErrorBanner.WriteException(SlkStore, sqlEx);
                            }
                            else
                            {
                                slkError = SlkError.WriteException(SlkStore, ex);
                            }                        

                            WebParts.ErrorBanner.RenderErrorItems(hw, slkError);
                        }

                    }
                }           
                
            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }
        }
        #endregion

        #region RenderQueryLinkRow
        /// <summary>
        /// Renders an HTML "&lt;tr&gt;" row containing one query link that's displayed within the
        /// left pane of ALWP.
        /// </summary>
        ///
        /// <param name="queryDef">Defines the query that this link will display when clicked.</param>
        ///
        /// <param name="queryIndex">The zero-based index of this query.</param>
        ///
        /// <param name="hw">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        static void RenderQueryLinkRow(QueryDefinition queryDef, int queryIndex, HtmlTextWriter hw)
        {
            // define the onclick handler for this row
            string onClickHandler = String.Format(CultureInfo.InvariantCulture, "javascript:SelectQuery({0}); window.focus(); return false;", queryIndex);

            // DHTML approach:  When a cell is selected, it's highlighted: its blue background becomes
            // white, and it gets a border.  However, we don't use cell borders because adding and
            // removing a border shifts text around, and we can't change the color of the border to
            // blue because we don't have a SharePoint style for that.  So, we "fake" the borders by
            // creating extra cells

            // write the top pseudo-border row
            hw.AddAttribute(HtmlTextWriterAttribute.Height, "1");
            using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
            {
                // write the left spacer cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                    HtmlBlock.WriteBlankGif("8", "1", hw);

                // write the left pseudo-border cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);

                // write the query label cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
                hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                String.Format(CultureInfo.InvariantCulture,                                                                                      "QueryLabelBorderTop{0}",
                                              queryIndex));
                HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);

                // write query count cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                String.Format(CultureInfo.InvariantCulture, 
                                              "QueryCountBorderTop{0}",
                                              queryIndex));
                HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);
            }

            // write the main row
            hw.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
            hw.AddAttribute(HtmlTextWriterAttribute.Class, "ms-navItem");
            hw.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Empty);
            using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
            {
                // write the left spacer cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                    HtmlBlock.WriteBlankGif("8", "1", hw);

                // write the left pseudo-border cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                String.Format(CultureInfo.InvariantCulture, 
                                "QueryLeftBorder{0}",
                                queryIndex));
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                    HtmlBlock.WriteBlankGif("1", "1", hw);

                // write query label cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
                hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                String.Format(CultureInfo.InvariantCulture, 
                                              "QueryLabelTD{0}",
                                              queryIndex));
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "padding: 3px 14px 3px 8px");
                hw.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Empty);
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                {
                    hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                    String.Format(CultureInfo.InvariantCulture, 
                                                  "QueryLabelA{0}",
                                                  queryIndex));
                    hw.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                    hw.AddAttribute(HtmlTextWriterAttribute.Onclick, onClickHandler);
                    using (new HtmlBlock(HtmlTextWriterTag.A, 0, hw))
                        hw.WriteEncodedText(queryDef.Title);
                }

                // write query count cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                String.Format(CultureInfo.InvariantCulture,     
                                              "QueryCountTD{0}",
                                              queryIndex));
                hw.AddAttribute(HtmlTextWriterAttribute.Style,
                    "padding: 3px 14px 3px 0px; border-right: solid 1px #E0E0E0; ");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                {
                    hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                    String.Format(CultureInfo.InvariantCulture,
                                    "QueryCountA{0}",
                                    queryIndex));
                    hw.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                    hw.AddAttribute(HtmlTextWriterAttribute.Onclick, onClickHandler);
                    using (new HtmlBlock(HtmlTextWriterTag.A, 0, hw))
                    {
                        hw.AddAttribute(HtmlTextWriterAttribute.Src, Constants.ImagePath + Constants.WaitIcon);
                        hw.AddAttribute(HtmlTextWriterAttribute.Width, "9");
                        hw.AddAttribute(HtmlTextWriterAttribute.Height, "13");
                        hw.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                        hw.AddAttribute(HtmlTextWriterAttribute.Alt, "");
                        HtmlBlock.WriteFullTag(HtmlTextWriterTag.Img, 0, hw);
                    }
                }
            }

            // write the bottom pseudo-border row
            hw.AddAttribute(HtmlTextWriterAttribute.Height, "1");
            using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, hw))
            {
                // write the left spacer cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, hw))
                    HtmlBlock.WriteBlankGif("8", "1", hw);

                // write the left pseudo-border cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width: 3px;");
                HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);

                // write the query label cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Style, "width:100%;");
                hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                String.Format(CultureInfo.InvariantCulture,
                                              "QueryLabelBorderBottom{0}",
                                              queryIndex));
                HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);

                // write query count cell
                hw.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                hw.AddAttribute(HtmlTextWriterAttribute.Id,
                                String.Format(CultureInfo.InvariantCulture, 
                                              "QueryCountBorderBottom{0}",
                                              queryIndex));
                HtmlBlock.WriteFullTag(HtmlTextWriterTag.Td, 1, hw);
            }
        }
        #endregion

        private string PageWithCulture(string basePage)
        {
            string culture = System.Web.HttpContext.Current.Request.QueryString["culture"];
            if (string.IsNullOrEmpty(culture))
            {
                return basePage;
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}?culture={1}", basePage, culture);
            }
        }

        private void WriteResultsForm(TextWriter writer)
        {
            WriteForm(writer, "QR", PageWithCulture(Constants.QueryResultPage), string.Empty);
        }

        private void WriteSummaryForm(TextWriter writer)
        {
            WriteForm(writer, "QSForm", PageWithCulture("QuerySummary.aspx"), "QS");
        }

        private void WriteForm(TextWriter writer, string id, string action, string target)
        {
            writer.Write("<form id=\"{0}{3}\" target=\"{0}{1}\" method=\"POST\" action=\"{2}\">", FrameId, target, action, id);
            writer.Write("<input id=\"alwp{0}{2}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.QuerySet, Request[QueryStringKeys.QuerySet], id);
            writer.Write("<input id=\"alwp{0}{2}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.Query, string.Empty, id);
            writer.Write("<input id=\"alwp{0}{2}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.Source, RawSourceUrl, id);
            writer.Write("<input id=\"alwp{0}{2}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.SPWebScope, string.Empty, id);
            writer.Write("<input id=\"alwp{0}{2}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.FrameId, FrameId, id);
            writer.Write("<input id=\"alwp{0}{2}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.Sort, string.Empty, id);

            if (IsObserver)
            {
                writer.Write("<input id=\"alwp{0}{2}\" name=\"{0}\" type=\"hidden\" value=\"{1}\"/>", QueryStringKeys.ForObserver, "true", id);
            }

            writer.Write("</form>");
        }
       
        #region RegisterQuerySetClientScriptBlock
        /// <summary>
        /// Registers the  onclick Client Script for QuerySet frame in the Response Stream.
        /// </summary>
        /// <param name="htmlTextWriter">Output Response Stream.</param>
   
        private void RegisterQuerySetClientScriptBlock(HtmlTextWriter htmlTextWriter)
        {            
            //Build the Script 
            StringBuilder csAlwpClientScript = new StringBuilder(1000);

            csAlwpClientScript.AppendLine("<!-- Place Holder Alwp Client Script -->");

            csAlwpClientScript.AppendLine(string.Format(CultureInfo.InvariantCulture, "var formResultsId = '{0}QR';", FrameId));

            csAlwpClientScript.AppendLine("<!--  Alwp Client Script Ends Here -->");

            //Add the Script in the Response Output 
            htmlTextWriter.Write(csAlwpClientScript.ToString());

        }
        #endregion
    }
}
