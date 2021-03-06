/* Copyright (c) Microsoft Corporation. All rights reserved. */

using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.SharePoint;
using Microsoft.SharePointLearningKit.WebControls;
using System.Text;
using Microsoft.SharePointLearningKit;
using Microsoft.LearningComponents.SharePoint;
using Microsoft.LearningComponents.Storage;
using Microsoft.LearningComponents;
using System.Security.Principal;
using Microsoft.LearningComponents.Manifest;
using System.Globalization;
using System.Collections.ObjectModel;
using Resources.Properties;
using System.Threading;
using System.IO;

namespace Microsoft.SharePointLearningKit.ApplicationPages
{
    /// <summary>
    /// Code-behind for Grading.aspx
    /// </summary>
    public class Grading : SlkAppBasePage
    {
        #region Control Declarations
#pragma warning disable 1591
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblPointsValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblStartValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblDueValue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblPoints;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblStart;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblDue;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblAutoReturn;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblAnswers;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblGradeAssignment;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblGradeAssignmentDescription;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblTitle;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "lbl")]
        protected Label lblDescription;
        protected Label lblError;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "btn")]
        protected Button btnTopSave;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "btn")]
        protected Button btnTopClose;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "btn")]
        protected Button btnBottomSave;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "btn")]
        protected Button btnBottomClose;

        protected SlkButton slkButtonEdit;
        protected SlkButton slkButtonCollect;
        protected SlkButton slkButtonReturn;
        protected SlkButton slkButtonDelete;

        protected Literal pageTitle;
        protected Literal pageTitleInTitlePage;
        protected Literal pageDescription;

        protected ErrorBanner errorBanner;
        protected GradingList gradingList;
        protected Panel contentPanel;
        protected Image infoImageAutoReturn;
        protected Image infoImageAnswers;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tbl")]
        protected HtmlTable tblAutoReturn;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tbl")]
        protected HtmlTable tblAnswers;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "tgr")]
        protected TableGridRow tgrAutoReturn;

        protected SlkButton slkButtonUpload;
        protected SlkButton slkButtonDownload;
#pragma warning restore 1591

        #endregion

        #region Private Variables
        /// <summary>
        /// Holds assignmentID value.
        /// </summary>
        private long? m_assignmentId;
        /// <summary>
        /// Holds Assignment Properties.
        /// </summary>
        private AssignmentProperties assignmentProperties;
        /// <summary>
        /// Keeps track if there was an error during one of the click events.
        /// </summary>
        private bool pageHasErrors;
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the AssignmentId Query String value.
        /// </summary>
        private long? AssignmentId
        {
            get
            {
                if (m_assignmentId == null)
                {
                    m_assignmentId = QueryString.ParseLong(QueryStringKeys.AssignmentId, null);
                }
                return m_assignmentId;
            }
        }
        /// <summary>
        /// Gets the AssignmentItemIdentifier value.
        /// </summary>
        private AssignmentItemIdentifier AssignmentItemIdentifier
        {
            get
            {
                AssignmentItemIdentifier assignmentItemId = null;
                if (AssignmentId != null)
                {
                    assignmentItemId = new AssignmentItemIdentifier(AssignmentId.Value);
                }
                return assignmentItemId;
            }
        }
        /// <summary>
        /// Gets the AssignmentProperties.
        /// </summary>
        private AssignmentProperties AssignmentProperties
        {
            set
            {
                assignmentProperties = value;
            }
            get
            {
                if (assignmentProperties == null)
                {
                    assignmentProperties = AssignmentProperties.LoadForGrading(AssignmentItemIdentifier, SlkStore);
                }
                return assignmentProperties;
            }
        }

        #endregion

        #region OnPreRender
        /// <summary>
        ///  Over rides OnPreRender.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                SetResourceText();
                LoadGradingList();

                if (!pageHasErrors)
                {

                    if (SPWeb.ID != AssignmentProperties.SPWebGuid)
                    {
                        Response.Redirect(SlkUtilities.UrlCombine(SPWeb.Url, Request.Path + "?" + Request.QueryString.ToString()));
                    }

                    AddReactivationCheck();

                    lblTitle.Text = Server.HtmlEncode(AssignmentProperties.Title);
                    lblDescription.Text = SlkUtilities.ClickifyLinks(Server.HtmlEncode(AssignmentProperties.Description).Replace("\r\n", "<br />\r\n"));

                    if (AssignmentProperties.PointsPossible.HasValue)
                    {
                        lblPointsValue.Text = AssignmentProperties.PointsPossible.Value.ToString(Constants.RoundTrip, NumberFormatInfo);
                    }
                    SPTimeZone timeZone = SPWeb.RegionalSettings.TimeZone;
                    lblStartValue.Text = FormatDateForDisplay(timeZone.UTCToLocalTime(AssignmentProperties.StartDate));

                    if (AssignmentProperties.DueDate.HasValue)
                    {
                        lblDueValue.Text = FormatDateForDisplay(timeZone.UTCToLocalTime(AssignmentProperties.DueDate.Value));
                    }

                    tblAutoReturn.Visible = AssignmentProperties.AutoReturn;
                    tblAnswers.Visible = AssignmentProperties.ShowAnswersToLearners;
                    tgrAutoReturn.Visible = tblAutoReturn.Visible || tblAnswers.Visible;
                }
            }
            catch (ThreadAbortException)
            {
                // make sure this exception isn't caught
                throw;
            }
            catch (Exception ex)
            {
                contentPanel.Visible = false;
                errorBanner.AddException(SlkStore, ex);
            }
        }
        #endregion

        /// <summary>
        /// Adds in code necessary to show a warning if the user is trying to reactivate an assignment.
        /// </summary>
        private void AddReactivationCheck()
        {
            StringBuilder script = new StringBuilder();
            script.AppendLine("function CheckReactivate()");
            script.AppendLine("{");
            script.AppendLine("\tvar showMessage = false;");
            script.AppendLine("\tfor (i=0;i<document.forms[0].elements.length;i++)");
            script.AppendLine("\t{");
            script.AppendLine("\t\tvar obj = document.forms[0].elements[i];");
            script.Append("\t\tif (obj.type == \"checkbox\" && obj.checked && obj.id.indexOf(\"chkAction\") > 0 && obj.parentElement.children[1].innerText == \"");
            script.Append(PageCulture.Resources.GradingActionTextFinal);
            script.AppendLine("\")");
            script.AppendLine("\t\t{");
            script.AppendLine("\t\t\tshowMessage = true;");
            script.AppendLine("\t\t}");
            script.AppendLine("\t}");
            script.AppendLine("\tif (showMessage)");
            script.Append("\t\treturn confirm(\"");
            script.Append(PageCulture.Resources.GradingReactivateMessage);
            script.AppendLine("\");");
            script.AppendLine("\telse");
            script.AppendLine("\t\treturn true;");
            script.AppendLine("}");
            ClientScript.RegisterClientScriptBlock(this.GetType(), "ReactivationCheck", script.ToString(), true);

            slkButtonEdit.OnClientClick = "return CheckReactivate();";
            btnTopSave.OnClientClick = "return CheckReactivate();";
            btnBottomSave.OnClientClick = "return CheckReactivate();";
        }

        /// <summary>
        /// Event handler for the OK button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "btn"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveGradingList(SaveAction.SaveOnly);
                // Make the page safe to refresh
                Response.Redirect(Request.RawUrl, true);
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
            catch (Exception ex)
            {
                pageHasErrors = true;
                errorBanner.AddException(SlkStore, ex);
            }
        }
        /// <summary>
        /// Event handler for the Cancel button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "btn"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void btnClose_Click(object sender, EventArgs e)
        {
            Response.Redirect(SPWeb.ServerRelativeUrl, true);
        }

        /// <summary>
        /// Event handler for the Edit button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void slkButtonEdit_Click(object sender, EventArgs e)
        {
            try
            {
                SaveGradingList(SaveAction.SaveOnly);

                string url = SlkUtilities.UrlCombine(SPWeb.ServerRelativeUrl, Constants.SlkUrlPath, "AssignmentProperties.aspx");
                url = String.Format(CultureInfo.InvariantCulture, "{0}?AssignmentId={1}", url, AssignmentId.ToString());

                Response.Redirect(url, true);
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
            catch (Exception ex)
            {
                pageHasErrors = true;
                errorBanner.AddException(SlkStore, ex);
            }
        }
        /// <summary>
        /// Event handler for the Collect button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void slkButtonCollect_Click(object sender, EventArgs e)
        {
            try
            {
                SaveGradingList(SaveAction.CollectAll);
                // Make the page safe to refresh
                Response.Redirect(Request.RawUrl, true);
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
            catch (Exception ex)
            {
                pageHasErrors = true;
                errorBanner.AddException(SlkStore, ex);
            }
        }
        /// <summary>
        /// Event handler for the Return button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void slkButtonReturn_Click(object sender, EventArgs e)
        {
            try
            {
                SaveGradingList(SaveAction.ReturnAll);
                // Make the page safe to refresh
                Response.Redirect(Request.RawUrl, true);
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
            catch (Exception ex)
            {
                pageHasErrors = true;
                errorBanner.AddException(SlkStore, ex);
            }
        }
        /// <summary>
        /// Event handler for the Delete button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void slkButtonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (SPWeb.ID == AssignmentProperties.SPWebGuid)
                {
                    Delete(SPWeb);
                }
                else
                {
                    // This should never be needed as page should always be in the context of the assignment's web.
                    using (SPSite spSite = new SPSite(AssignmentProperties.SPSiteGuid, SPContext.Current.Site.Zone))
                    {
                        using (SPWeb spWeb = spSite.OpenWeb(AssignmentProperties.SPWebGuid))
                        {
                            Delete(spWeb);
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Calling Response.Redirect throws a ThreadAbortException which will
                // flag an error in the next step if we don't do this.
                throw;
            }
            catch (Exception ex)
            {
                pageHasErrors = true;
                contentPanel.Visible = false;
                errorBanner.AddException(SlkStore, ex);
            }
        }

        void Delete(SPWeb web)
        {
            AssignmentProperties.Delete(web);
            string redirectUrl = web.ServerRelativeUrl;
            Response.Redirect(redirectUrl, true);
        }

        #region SetResourceText
        /// <summary>
        ///  Set the Control Text from Resource File.
        /// </summary>
        private void SetResourceText()
        {
            lblPoints.Text = PageCulture.Resources.GradinglblPoints;
            lblStart.Text = PageCulture.Resources.GradinglblStart;
            lblDue.Text = PageCulture.Resources.GradinglblDue;
            lblAutoReturn.Text = PageCulture.Resources.GradinglblAutoReturn;
            lblAnswers.Text = PageCulture.Resources.GradinglblAnswers;
            lblGradeAssignment.Text = PageCulture.Resources.GradinglblGradeAssignment;
            lblGradeAssignmentDescription.Text = PageCulture.Resources.GradinglblGradeAssignmentDescription;

            pageTitle.Text = PageCulture.Resources.GradingPageTitle;
            pageTitleInTitlePage.Text = PageCulture.Resources.GradingPageTitleinTitlePage;
            pageDescription.Text = PageCulture.Resources.GradingPageDescription;

            slkButtonEdit.Text = PageCulture.Resources.GradingEditText;
            slkButtonEdit.ToolTip = PageCulture.Resources.GradingEditToolTip;
            slkButtonEdit.ImageUrl = Constants.ImagePath + Constants.EditIcon;
            slkButtonEdit.AccessKey = PageCulture.Resources.GradingEditAccessKey;

            slkButtonCollect.Text = PageCulture.Resources.GradingCollectText;
            slkButtonCollect.ToolTip = PageCulture.Resources.GradingCollectToolTip;
            slkButtonCollect.OnClientClick = String.Format(CultureInfo.InvariantCulture, "javascript: return confirm('{0}');", PageCulture.Resources.GradingCollectMessage);
            slkButtonCollect.ImageUrl = Constants.ImagePath + Constants.CollectAllIcon;
            slkButtonCollect.AccessKey = PageCulture.Resources.GradingCollectAccessKey;

            slkButtonReturn.Text = PageCulture.Resources.GradingReturnText;
            slkButtonReturn.ToolTip = PageCulture.Resources.GradingReturnToolTip;
            slkButtonReturn.OnClientClick = String.Format(CultureInfo.InvariantCulture, "javascript: return confirm('{0}');", PageCulture.Resources.GradingReturnMessage);
            slkButtonReturn.ImageUrl = Constants.ImagePath + Constants.ReturnAllIcon;
            slkButtonReturn.AccessKey = PageCulture.Resources.GradingReturnAccessKey;

            slkButtonDelete.Text = PageCulture.Resources.GradingDeleteText;
            slkButtonDelete.ToolTip = PageCulture.Resources.GradingDeleteToolTip;
            slkButtonDelete.OnClientClick = String.Format(CultureInfo.InvariantCulture, "javascript: return confirm('{0}');", PageCulture.Resources.GradingDeleteMessage);
            slkButtonDelete.ImageUrl = Constants.ImagePath + Constants.DeleteIcon;
            slkButtonDelete.AccessKey = PageCulture.Resources.GradingDeleteAccessKey;

            btnTopSave.Text = PageCulture.Resources.GradingSave;
            btnTopClose.Text = PageCulture.Resources.GradingClose;
            btnBottomSave.Text = PageCulture.Resources.GradingSave;
            btnBottomClose.Text = PageCulture.Resources.GradingClose;

            infoImageAnswers.AlternateText = PageCulture.Resources.SlkErrorTypeInfoToolTip;
            infoImageAutoReturn.AlternateText = PageCulture.Resources.SlkErrorTypeInfoToolTip;

            slkButtonUpload.Text = PageCulture.Resources.GradingUploadText;
            slkButtonUpload.ToolTip = PageCulture.Resources.GradingUploadToolTip;
            slkButtonUpload.ImageUrl = Constants.ImagePath + Constants.UploadIcon;
            slkButtonUpload.AccessKey = PageCulture.Resources.GradingUploadAccessKey;

            slkButtonDownload.Text = PageCulture.Resources.GradingDownloadText;
            slkButtonDownload.ToolTip = PageCulture.Resources.GradingDownloadToolTip;
            slkButtonDownload.ImageUrl = Constants.ImagePath + Constants.DownloadIcon;
            slkButtonDownload.AccessKey = PageCulture.Resources.GradingDownloadAccessKey;
        }
        #endregion

        #region SaveGradingList
        /// <summary>
        /// Saves the info from the grading list
        /// </summary>
        /// <param name="action">Determines how the learner assignment statud should be handled.</param>
        private void SaveGradingList(SaveAction action)
        {
            // gradingList.DeterminePostBackGradingItems() only returns the rows that have changed
            Dictionary<string, GradingItem> gradingListItems = gradingList.DeterminePostBackGradingItems();
            List<LearnerAssignmentProperties> gradingPropertiesList = new List<LearnerAssignmentProperties>();

            if (action == SaveAction.CollectAll || action == SaveAction.ReturnAll)
            {
                foreach (GradingItem item in gradingList.Items.Values)
                {
                    switch (action)
                    {
                        case SaveAction.CollectAll:
                            if (item.Status == LearnerAssignmentState.NotStarted || item.Status == LearnerAssignmentState.Active)
                            {
                                gradingListItems[item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture)] = item;
                            }
                            break;
                        case SaveAction.ReturnAll:
                            if (item.Status != LearnerAssignmentState.Final)
                            {
                                gradingListItems[item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture)] = item;
                            }
                            break;
                    }
                }
            }

            if (gradingListItems.Count > 0)
            {
                using (AssignmentSaver saver = AssignmentProperties.CreateSaver())
                {
                    bool hasSaved = false;
                    try
                    {
                        foreach (GradingItem item in gradingListItems.Values)
                        {
                            bool moveStatusForward = false;
                            bool returnAssignment = false;
                            LearnerAssignmentProperties gradingProperties = AssignmentProperties[item.LearnerAssignmentId];
                            gradingProperties.FinalPoints = item.FinalScore;
                            gradingProperties.Grade = item.Grade;
                            gradingProperties.InstructorComments = item.InstructorComments;

                            // Ignore the FinalScore Update if the Status is NotStarted or Active
                            if (item.Status == LearnerAssignmentState.NotStarted || item.Status == LearnerAssignmentState.Active)
                            {
                                gradingProperties.IgnoreFinalPoints = true;
                            }

                            switch (action)
                            {
                                case SaveAction.SaveOnly:
                                    // The Save or OK button was clicked
                                    moveStatusForward = item.ActionState;
                                    break;

                                case SaveAction.CollectAll:
                                    // The Collect All button was clicked
                                    if (item.Status == LearnerAssignmentState.NotStarted || item.Status == LearnerAssignmentState.Active)
                                    {
                                        moveStatusForward = true;
                                    }
                                    break;

                                case SaveAction.ReturnAll:
                                    if (item.Status != LearnerAssignmentState.Final)
                                    {
                                        returnAssignment = true;
                                    }
                                    break;
                            }

                            gradingProperties.Save(moveStatusForward, returnAssignment, saver);
                        }

                        hasSaved = true;
                        saver.Save();
                    }
                    catch
                    {
                        if (hasSaved == false)
                        {
                            saver.Cancel();
                        }

                        throw;
                    }
                }
            }
        }

        #endregion

        #region LoadGradingList
        /// <summary>
        /// Loads the grading list control
        /// </summary>
        private void LoadGradingList()
        {
            try
            {
                gradingList.AssignmentProperties = AssignmentProperties;
                gradingList.Initialize(SlkStore.Settings);
            }
            catch (InvalidOperationException exception)
            {
                SlkStore.LogException(exception);
                throw new SafeToDisplayException(PageCulture.Resources.GradingInvalidAssignmentId, AssignmentId);
            }
        }
        #endregion

        private enum SaveAction
        {
            SaveOnly,
            CollectAll,
            ReturnAll
        }
    }
}
