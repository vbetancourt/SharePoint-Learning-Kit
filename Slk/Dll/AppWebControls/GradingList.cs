/* Copyright (c) Microsoft Corporation. All rights reserved. */

//GradingList.cs
//
//Implementation of Grade the assignment section, ServerControl and associated Classes for GradingList control 
//
using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Microsoft.SharePointLearningKit;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Frameset;
using Resources.Properties;
using System.Text;
using System.Globalization;


namespace Microsoft.SharePointLearningKit.WebControls
{
    /// <summary>
    ///  Custom Control to Render Grading Items which is used to 
    ///  enter grades and comments for each learner assignment, 
    ///  and advance the workflow state for a learner assignment.
    ///  usage: &lt;{0}:GradingList runat="server" ID="gradingList"&gt;
    ///         &lt;/{0}:GradingList&gt;;
    /// </summary>

    [ToolboxData("<{0}:GradingList runat=server></{0}:GradingList>")]
    public class GradingList : WebControl, INamingContainer
    {

        #region Private Fields
        /// <summary>
        ///  Collection of Grading Items.
        /// </summary>
        private GradingItemCollection m_items;
        /// <summary>
        /// Holds the Postback Grading Item Collection.
        /// </summary>
        private Dictionary<string, GradingItem> m_postBackGradingItems;
        /// <summary>
        /// Holds whether the Assignment is from Class Server Content or not.
        /// </summary>
        private bool m_isClassServerContent;

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor used to initialize the Grading Item Collection  
        /// </summary>
        public GradingList()
        {
            //Initialize the GradingItem Collection
            this.m_items = new GradingItemCollection();
        }
        #endregion

        #region Public and Private Properties
        /// <summary>
        /// Returns Unique ID to the Control.
        /// </summary>
        public override string UniqueID
        {
            get
            {
                return base.UniqueID;
            }
        }
        /// <summary>
        /// FinalPoints Field TextBox Control ID
        /// </summary>
        private string FinalScoreId
        {
            get
            {
                return this.ClientID + ClientIDSeparator
                       + "txtFinalScore" + ClientIDSeparator;
            }
        }
        /// <summary>
        /// Comments Field TextBox Control ID
        /// </summary>
        private string CommentsId
        {
            get
            {
                return this.ClientID + ClientIDSeparator
                       + "txtComments" + ClientIDSeparator;
            }
        }
        /// <summary>
        /// Action Field CheckBox Control ID
        /// </summary>
        private string ActionId
        {
            get
            {
                return this.ClientID + ClientIDSeparator
                       + "chkAction" + ClientIDSeparator;
            }
        }

        /// <summary>
        /// Grading Score Control ID
        /// </summary>
        private string GradingScoreId
        {
            get
            {
                return this.ClientID + ClientIDSeparator
                       + "lblGradedScore" + ClientIDSeparator;
            }
        }

        /// <summary>
        /// Grading Image Control ID
        /// </summary>
        private string GradingImageId
        {
            get
            {
                return this.ClientID + ClientIDSeparator
                       + "imgGraded" + ClientIDSeparator;
            }
        }

        /// <summary>
        /// Grading Row ID
        /// </summary>
        private string GradingRowId
        {
            get
            {
                return this.ClientID + ClientIDSeparator
                       + "gradingRow" + ClientIDSeparator;
            }
        }

        /// <summary>
        /// Collection of Grading Items to be listed
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        internal GradingItemCollection Items
        {
            get
            {
                return m_items;
            }
        }
        /// <summary>
        /// Holds whether the Assignment is from Class Server Content or not.
        /// </summary>
        internal bool IsClassServerContent
        {
            get { return m_isClassServerContent; }
            set { m_isClassServerContent = value; }
        }

        #endregion

        #region Private and Protected Methods

        #region OnInit
        /// <summary>
        /// Override OnInit to Register ControlState
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }
        #endregion

        #region LoadControlState
        /// <summary>
        /// Loads Control State from previously saved State
        /// </summary>
        /// <param name="savedState">savedState</param>
        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                Pair p = (Pair)savedState;
                base.LoadViewState(p.First);
                Items.LoadViewState(p.Second);
            }
            else
            {
                base.LoadViewState(null);
            }
            //base.LoadControlState(savedState);
        }

        #endregion

        #region Add
        /// <summary>
        /// Adds the GradingItem to the GradingItemCollection             
        /// </summary>
        /// <param name="item">GradingItem</param> 
        private void Add(GradingItem item)
        {
            //adds the GradingItem to the GradingItemCollection
            this.m_items.Add(item);
        }
        #endregion

        #region Add
        /// <summary>
        /// Adds the GradingProperties to the GradingItemCollection             
        /// </summary>
        /// <param name="gradingProperties">Grading Properties</param> 
        public void Add(GradingProperties gradingProperties)
        {
            GradingItem item = new GradingItem();

            item.LearnerAssignmentId
                        = gradingProperties.LearnerAssignmentId.GetKey();
            item.GradedScore
                        = gradingProperties.GradedPoints;
            item.FinalScore = gradingProperties.FinalPoints;
            item.InstructorComments = gradingProperties.InstructorComments;
            item.LearnerName = gradingProperties.LearnerName;
            item.Status = gradingProperties.Status.GetValueOrDefault();
            item.SuccessStatus = gradingProperties.SuccessStatus;


            //adds the GradingItem to the GradingItemCollection
            Add(item);
        }
        #endregion

        #region OnPreRender
        /// <summary>
        ///  Over rides OnPreRender to Render APP 
        /// </summary> 
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //Register Client Script
            RegisterGradingClientScriptBlock();

        }
        #endregion+

        #region Render
        /// <summary>
        /// Renders the Grading List Collection
        /// </summary>
        /// <param name="writer">htmlTextWriter</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Items != null && Items.Count > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                using (new HtmlBlock(HtmlTextWriterTag.Table, 1, writer))
                {
                    using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, writer))
                    {
                        using (new HtmlBlock(HtmlTextWriterTag.Td, 0, writer))
                        {
                            // render the "<table>" element and its contents 
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-summarystandardbody");
                            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "1");
                            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, writer))
                            {
                                // render the header row 
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-viewheadertr");
                                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                                using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, writer))
                                {
                                    // render the Learner column headers
                                    RenderColumnHeader(AppResources.GradingLearnerHeaderText, writer);
                                    // render the Status column headers
                                    RenderColumnHeader(AppResources.GradingStatusHeaderText, writer);
                                    // render the Graded Score column headers
                                    RenderColumnHeader(AppResources.GradingGradedScoreHeaderText, writer);
                                    // render the Final Score column headers
                                    RenderColumnHeader(AppResources.GradingFinalScoreHeaderText, writer);
                                    // render the Comments column headers
                                    RenderColumnHeader(AppResources.GradingCommentsHeaderText, writer);
                                    // render the Action column headers
                                    RenderColumnHeader(AppResources.GradingActionHeaderText, writer);
                                }
                                foreach (GradingItem item in Items)
                                {
                                    RenderGradingItem(item, writer);
                                }
                            }
                        }
                    }
                }

            }

        }
        #endregion

        #region RenderColumnHeader
        /// <summary>
        /// Renders a column header, i.e. the label at the top of a column in the Grading list.
        /// </summary>
        /// 
        /// <param name="columnName">The <c>Name</c> of the column being rendered.</param>
        /// 
        /// <param name="htmlTextWriter">The <c>HtmlTextWriter</c> to write to.</param>
        ///
        private static void RenderColumnHeader(string columnName, HtmlTextWriter htmlTextWriter)
        {
            // render the "<th>" element for this column header
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vh");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "border-left: none; padding-left: 3px;");
            using (new HtmlBlock(HtmlTextWriterTag.Th, 1, htmlTextWriter))
            {
                // render the "<div>" element containing the column header
                htmlTextWriter.WriteLine();
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                            "position: relative; left: 0px; top: 0px; width: 100%;");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Div, 1, htmlTextWriter))
                {
                    // render the "<table>" element containing the column header
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-unselectedtitle");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "width: 100%;");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Height, "100%");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    using (new HtmlBlock(HtmlTextWriterTag.Table, 0, htmlTextWriter))
                    {
                        using (new HtmlBlock(HtmlTextWriterTag.Tr, 0, htmlTextWriter))
                        {
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                            using (new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter))
                            {
                                // write the column title
                                htmlTextWriter.WriteEncodedText(columnName);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region RenderGradedLearner
        /// <summary>
        /// Render the Graded Learner 
        /// </summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderGradedLearner(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            //If LearnerAssignmentState is NotStarted , No Link
            //ToolTip Similar to: The learner has not started the assignment
            if (item.Status == LearnerAssignmentState.NotStarted)
            {
                Label lblLearnerItem = new Label();
                lblLearnerItem.ID = "lblLearner" + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                lblLearnerItem.Text = item.LearnerName;
                lblLearnerItem.ToolTip = AppResources.GradingStatusNotStartedToolTip;
                lblLearnerItem.RenderControl(htmlTextWriter);
            }
            else
            {
                // If the assignment has an e-learning payload, 
                // the learner�s name is hyperlinked to open the learner assignment in the frameset. 

                HyperLink lnkLearnerItem = new HyperLink();
                lnkLearnerItem.ID = "lnkLearner" + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                lnkLearnerItem.Text = item.LearnerName;

                //Final Score is disabled for Class Server content when the frameset 
                //is open to the grading view of the associated learner assignment.
                switch (item.Status)
                {
                    //If LearnerAssignmentState is  Active, Opens frameset in a new non-named window 
                    //in Instructor Review View.
                    //ToolTip Similar to: View learner work in progress  
                    case LearnerAssignmentState.Active:
                        lnkLearnerItem.ToolTip = AppResources.GradingStatusInProgressToolTip;
                        lnkLearnerItem.NavigateUrl
                            = String.Format(
                                       CultureInfo.InvariantCulture,
                                       "javascript:Slk_DisableGradingFinalScore(\"{0}?{1}={2}&{3}={4}\",{4},{5});",
                                       Constants.FrameSetPage,
                                       FramesetQueryParameter.SlkView,
                                       Frameset.AssignmentView.InstructorReview,
                                       FramesetQueryParameter.LearnerAssignmentId,
                                       item.LearnerAssignmentId,
                                       IsClassServerContent ? "true" : "false");
                        break;
                    //If LearnerAssignmentState is Completed, Opens frameset in a new non-named window 
                    //in Grading View.
                    //ToolTip Similar to: Grade learner work 
                    case LearnerAssignmentState.Completed:
                        lnkLearnerItem.ToolTip = AppResources.GradingStatusSubmittedToolTip;
                        lnkLearnerItem.NavigateUrl
                            = String.Format(
                                       CultureInfo.InvariantCulture,
                                       "javascript:Slk_DisableGradingFinalScore(\"{0}?{1}={2}&{3}={4}&{6}={7}\",{4},{5});",
                                       Constants.FrameSetPage,
                                       FramesetQueryParameter.SlkView,
                                       Frameset.AssignmentView.Grading,
                                       FramesetQueryParameter.LearnerAssignmentId,
                                       item.LearnerAssignmentId,
                                       IsClassServerContent ? "true" : "false",
                                       FramesetQueryParameter.Src,
                                       "Grading");
                        break;
                    //If LearnerAssignmentState is Final, Opens frameset in a new non-named window 
                    //in Grading View.
                    //ToolTip Similar to: View and Grade returned work
                    case LearnerAssignmentState.Final:
                        lnkLearnerItem.ToolTip = AppResources.GradingStatusFinalToolTip;
                        lnkLearnerItem.NavigateUrl
                            = String.Format(
                                    CultureInfo.InvariantCulture,
                                    "javascript:Slk_DisableGradingFinalScore(\"{0}?{1}={2}&{3}={4}&{6}={7}\",{4},{5});",
                                    Constants.FrameSetPage,
                                    FramesetQueryParameter.SlkView,
                                    Frameset.AssignmentView.Grading,
                                    FramesetQueryParameter.LearnerAssignmentId,
                                    item.LearnerAssignmentId,
                                    IsClassServerContent ? "true" : "false",
                                    FramesetQueryParameter.Src,
                                    "Grading");

                        break;
                }
                lnkLearnerItem.RenderControl(htmlTextWriter);
            }
        }
        #endregion

        #region RenderGradedScore
        /// <summary>
        /// Render the Graded Score which Shows the computed points, 
        /// rounded to the nearest numeric, followed by an diamond icon 
        /// if the learner assignment success status is known. 
        /// </summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderGradedScore(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, htmlTextWriter))
            {
                //Render the computed points, rounded to the nearest integer,
                //followed by an icon which tell the success status.

                using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
                {
                    //Controls to Graded Score
                    Image imgGraded = new Image();
                    imgGraded.ID
                        = GradingImageId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                    imgGraded.Width = new Unit(11, UnitType.Pixel);
                    imgGraded.Height = new Unit(11, UnitType.Pixel);

                    //Render Graded Score
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-right: 6px;");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter))
                    {
                        Label lblGradedScore = new Label();
                        lblGradedScore.ID
                            = GradingScoreId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                        //Add the ToolTipText to get the localized Text while 
                        //Changing the Tool Tip from Client Side 
                        lblGradedScore.Attributes.Add("ToolTipText", AppResources.GradingScoreToolTip);
                        if (item.GradedScore != null)
                        {
                            lblGradedScore.Text
                                = String.Format(CultureInfo.CurrentCulture, AppResources.GradingGradedScore, item.GradedScore.Value);
                            //Tool Tip for Graded Score 
                            //Similar to <Computed points with full precision> Points.
                            lblGradedScore.ToolTip = String.Format(CultureInfo.CurrentCulture, AppResources.GradingGradedScoreToolTip,
                                                                    item.GradedScore.Value, AppResources.GradingScoreToolTip);
                        }
                        else
                        {
                            lblGradedScore.Text = Constants.NonBreakingSpace;
                        }

                        lblGradedScore.RenderControl(htmlTextWriter);
                    }

                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "1%");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                                "padding-right: 6px; padding-top:2px");
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                    using (new HtmlBlock(HtmlTextWriterTag.Td, 0, htmlTextWriter))
                    {
                        //Image Tag icon for the learner assignment success status              
                        switch (item.SuccessStatus)
                        {
                            case SuccessStatus.Unknown:
                                imgGraded.ImageUrl = Constants.BlankGifUrl;
                                break;
                            case SuccessStatus.Failed:
                                imgGraded.ImageUrl = Constants.ImagePath + Constants.GradingUnSatisfiedIcon;
                                imgGraded.ToolTip = AppResources.GradingSuccessStatusUnSatisfiedToolTip;
                                break;
                            case SuccessStatus.Passed:

                                imgGraded.ImageUrl = Constants.ImagePath + Constants.GradingSatisfiedIcon;
                                imgGraded.ToolTip = AppResources.GradingSuccessStatusSatisfiedToolTip;
                                break;
                        }
                        //Render the Image Control
                        imgGraded.RenderControl(htmlTextWriter);
                    }
                }
            }
        }
        #endregion

        #region RenderFinalScore
        /// <summary>
        /// Render the Final Score defaults to the computed points value, 
        /// and always shows full precision.
        /// </summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderFinalScore(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            //Renders  the computed points value, and always shows full precision.

            string uniqueId = FinalScoreId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);

            TextBox txtFinalScore = new TextBox();
            txtFinalScore.CssClass = "ms-input";
            txtFinalScore.Width = new Unit(50, UnitType.Pixel);
            txtFinalScore.ID = uniqueId;
            string onFocusHandler
                = String.Format(CultureInfo.InvariantCulture,
                                "Slk_GradingHighlightGradingRow({0});",
                                item.LearnerAssignmentId);

            //Add Client Event Handlers to the Control
            txtFinalScore.Attributes.Add("onblur", "Slk_GradingValidateFinalScore(this);");
            txtFinalScore.Attributes.Add("onclick", "Slk_GradingFinalScoreDisabled(this);");
            txtFinalScore.Attributes.Add("onfocus", onFocusHandler);
            if (item.FinalScore != null)
            {
                txtFinalScore.Text = item.FinalScore.Value.ToString(CultureInfo.CurrentCulture);
            }

            switch (item.Status)
            {
                //If LearnerAssignmentState is NotStarted or In Progress, Behavior Disabled
                //ToolTip Similar to: The learner has not submitted the assignment. 
                //To assign a grade, you must collect it.
                case LearnerAssignmentState.NotStarted:
                    txtFinalScore.ToolTip = AppResources.GradingFinalScoreNotSubmittedToolTip;
                    txtFinalScore.Enabled = false;
                    break;
                case LearnerAssignmentState.Active:
                    txtFinalScore.ToolTip = AppResources.GradingFinalScoreNotSubmittedToolTip;
                    txtFinalScore.Enabled = false;
                    break;
                //If LearnerAssignmentState is Submitted, Final, Behavior Enabled
                //ToolTip Similar to: To override autograding, enter a final score here.
                case LearnerAssignmentState.Completed:
                    txtFinalScore.ToolTip = AppResources.GradingFinalScoreSubmittedToolTip;
                    break;
                case LearnerAssignmentState.Final:
                    txtFinalScore.ToolTip = AppResources.GradingFinalScoreSubmittedToolTip;
                    break;
                default:
                    break;
            }

            //Render the Final Score control only in the Submitted and Final states
            if (txtFinalScore.Enabled)
            {
                txtFinalScore.RenderControl(htmlTextWriter);
            }

        }
        #endregion

        #region RenderActionCheckBox
        /// <summary>
        /// Each row of the grading Grid has an action checkbox that can be used 
        /// to advance the learner assignment to the next state in the workflow. 
        /// The label and tooltip of the checkbox reflect the state that 
        /// the learner assignment would advance to.
        /// </summary>
        /// <param name="item">Item to be rendered</param>
        /// <param name="htmlTextWriter">Text Writer to write to.</param>
        private void RenderActionCheckBox(GradingItem item, HtmlTextWriter htmlTextWriter)
        {
            string uniqueId
                = ActionId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
            string actionToolTip = String.Empty;

            switch (item.Status)
            {
                //If LearnerAssignmentState is NotStarted or Active, checkbox label is "Collect", 
                //Tooltip is "Prevents the learner from  continuing to work on the assignment, 
                //and allows you to grade it".
                case LearnerAssignmentState.NotStarted:
                    item.ActionText = AppResources.GradingActionTextNotSubmitted;
                    actionToolTip = AppResources.GradingActionNotSubmittedToolTip;
                    break;
                case LearnerAssignmentState.Active:
                    item.ActionText = AppResources.GradingActionTextNotSubmitted;
                    actionToolTip = AppResources.GradingActionNotSubmittedToolTip;
                    break;
                //If LearnerAssignmentState is Completed, checkbox label is "Return", 
                //Tooltip is "Returns the assignment to the learner for  review"
                case LearnerAssignmentState.Completed:
                    item.ActionText = AppResources.GradingActionTextSubmitted;
                    actionToolTip = AppResources.GradingActionSubmittedToolTip;
                    break;
                //If LearnerAssignmentState is Final, checkbox label is "Reactivate",
                //  Tooltip is "Allows the learner to re-do the assignment."
                case LearnerAssignmentState.Final:
                    item.ActionText = AppResources.GradingActionTextFinal;
                    actionToolTip = AppResources.GradingActionFinalToolTip;
                    break;
                default:
                    break;
            }

            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
            using (new HtmlBlock(HtmlTextWriterTag.Table, 1, htmlTextWriter))
            {
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "SlkGrading");
                    using (new HtmlBlock(HtmlTextWriterTag.Div, 1, htmlTextWriter))
                    {
                        //Render Checkbox
                        CheckBox checkBox = new CheckBox();
                        checkBox.ID = uniqueId;
                        checkBox.ToolTip = actionToolTip;
                        checkBox.Checked = item.ActionState;
                        string onFocusHandler
                            = String.Format(CultureInfo.InvariantCulture,
                                            "Slk_GradingHighlightGradingRow({0});",
                                            item.LearnerAssignmentId);
                        checkBox.Attributes.Add("onfocus", onFocusHandler);
                        checkBox.Text = item.ActionText;
                        checkBox.RenderControl(htmlTextWriter);
                    }

                }
            }
        }
        #endregion

        #region RenderGradingItem
        /// <summary>
        /// Render the Grading Item Row.  
        /// </summary>       
        /// <param name="item">GradingItem</param>
        /// <param name="htmlTextWriter">htmlTextWriter</param>
        private void RenderGradingItem(GradingItem item, HtmlTextWriter htmlTextWriter)
        {

            string onClickHandler
                = String.Format(CultureInfo.InvariantCulture,
                                "Slk_GradingHighlightGradingRow({0});",
                                item.LearnerAssignmentId);

            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Onclick, onClickHandler);
            htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Id,
                                        String.Format(CultureInfo.InvariantCulture,
                                                      "{0}{1}", GradingRowId, item.LearnerAssignmentId));
            using (new HtmlBlock(HtmlTextWriterTag.Tr, 1, htmlTextWriter))
            {
                //Render Graded Learner 
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-left: 5px; padding-top:5pt");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    RenderGradedLearner(item, htmlTextWriter);
                }
                //Renders the status of the learner assignment
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-left: 5px; padding-top:5pt");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    htmlTextWriter.Write(SlkUtilities.GetLearnerAssignmentState(item.Status));
                }
                //Render the Graded Score 
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                    "width: 1%; padding-left: 5px; padding-top:5pt");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    RenderGradedScore(item, htmlTextWriter);
                }
                //Render Final Score   
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                            "width: 50px; padding-left: 5px; padding-top:3px");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {

                    RenderFinalScore(item, htmlTextWriter);
                }
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style,
                                            "width: 200px; padding-left: 5px; padding-top:3px");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                //Render Comments  
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {
                    TextBox txtInstructorComments = new TextBox();
                    txtInstructorComments.CssClass = "ms-long";
                    txtInstructorComments.ID
                        = CommentsId + item.LearnerAssignmentId.ToString(CultureInfo.InvariantCulture);
                    txtInstructorComments.TextMode = TextBoxMode.MultiLine;
                    txtInstructorComments.Style.Value = "width: 100%; overflow:visible";
                    txtInstructorComments.Text = item.InstructorComments;
                    string onFocusHandler
                        = String.Format(CultureInfo.InvariantCulture,
                                        "Slk_GradingHighlightGradingRow({0});",
                                        item.LearnerAssignmentId);
                    txtInstructorComments.Attributes.Add("onfocus", onFocusHandler);
                    txtInstructorComments.RenderControl(htmlTextWriter);

                }
                //Render Action checkboxes
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Style, "padding-left: 3px;");
                htmlTextWriter.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
                using (new HtmlBlock(HtmlTextWriterTag.Td, 1, htmlTextWriter))
                {

                    RenderActionCheckBox(item, htmlTextWriter);
                }
            }
        }
        #endregion

        #region SetGradingItems
        /// <summary>
        ///  Set the Postback Collection to Grading Items
        /// </summary>
        /// <param name="controlId">Id</param>
        /// <param name="controlValue">Value</param>       
        private void SetGradingItems(string controlId, string controlValue)
        {

            //Find  the Control and update the corresponding Grading Item   

            GradingItem item = null;

            bool isGradingItemChanged = false;
            string key = string.Empty;

            //set the Final Score Value
            if (controlId.StartsWith(FinalScoreId, StringComparison.Ordinal))
            {
                key = controlId.Substring(FinalScoreId.Length);

                GetGradingItem(key, out item);
                float? finalScore = null;

                if (!String.IsNullOrEmpty(controlValue))
                {
                    try
                    {
                        finalScore = float.Parse(controlValue, CultureInfo.CurrentCulture.NumberFormat);
                    }
                    catch (FormatException ex)
                    {
                        throw new SafeToDisplayException(ex.Message, ex);
                    }
                }

                if (item.FinalScore != finalScore)
                {
                    isGradingItemChanged = true;
                    item.FinalScore = finalScore;
                }

            }  //set the Comments Value
            else if (controlId.StartsWith(CommentsId, StringComparison.Ordinal))
            {
                key = controlId.Substring(CommentsId.Length);
                GetGradingItem(key, out item);

                if (item.InstructorComments != controlValue)
                {
                    isGradingItemChanged = true;
                    item.InstructorComments = controlValue;
                }

            } //set the Action Value
            else if (controlId.StartsWith(ActionId, StringComparison.Ordinal))
            {
                key = controlId.Substring(ActionId.Length);

                GetGradingItem(key, out item);

                bool actionState = (controlValue == "on") ?
                    true : false;

                if (item.ActionState != actionState)
                {
                    isGradingItemChanged = true;
                    item.ActionState = actionState;
                }
            }

            //If the Postback Collection has changes add the modfied Item 
            //to the Postback Grading Item Collection to return and also Check if 
            //Item already exists in Postback Grading Item Collection.

            if (isGradingItemChanged &&
                !(m_postBackGradingItems.ContainsKey(key)))
            {
                m_postBackGradingItems.Add(key, item);
            }
        }
        #endregion

        #region DeterminePostBackGradingItems
        /// <summary>
        /// Determines PostBack Collection of GradingItems and 
        /// returns the Grading Item Collection as Dictionary
        /// </summary>
        /// <returns>Dictionary Collection of Grading Item</returns>
        internal Dictionary<string, GradingItem> DeterminePostBackGradingItems()
        {
            m_postBackGradingItems = new Dictionary<string, GradingItem>();

            NameValueCollection postbackCollection = this.Page.Request.Form;

            foreach (string key in postbackCollection.Keys)
            {
                if (key.Contains(this.ClientID))
                {
                    string controlID = key;
                    string controlValue = postbackCollection[key];
                    SetGradingItems(controlID, controlValue);
                }
            }

            return m_postBackGradingItems;

        }

        #endregion

        #region GetGradingItem
        /// <summary>
        ///  Outs the Grading Item 
        /// </summary>
        /// <param name="key">Key to Identify the Item</param>
        /// <param name="item">GradingItem</param>
        private void GetGradingItem(string key, out GradingItem item)
        {

            if (!m_postBackGradingItems.TryGetValue(key, out item))
            {
                long value = long.Parse(key, CultureInfo.CurrentCulture.NumberFormat);

                item = this.Items.FindByValue(value);

                if (item == null)
                {
                    throw new ArgumentNullException("item");
                }
            }
        }
        #endregion

        #region SaveControlState
        /// <summary>
        /// Save the ControlState and returns the saved object
        /// </summary>
        /// <returns>savedObject</returns>
        protected override object SaveControlState()
        {
            object baseState = base.SaveControlState();
            object itemState = Items.SaveViewState();
            if ((baseState == null) && (itemState == null))
                return null;
            return new Pair(baseState, itemState);
        }
        #endregion

        #region Clear
        /// <summary>
        /// Removes all the GradingItem in the Grading Item Collection 
        /// </summary>      
        internal void Clear()
        {
            //Clear the Grading Item Collection
            this.Items.Clear();

        }
        #endregion

        #region RegisterGradingClientScriptBlock
        /// <summary>
        /// Method that creates and defines the Grading Client Script  
        /// and registers script blocks with the page.
        /// </summary>    
        private void RegisterGradingClientScriptBlock()
        {
            // Define the name and type of the client scripts on the page.

            String csTitle = "GradingClientScript";

            Type cstype = this.GetType();

            // Get a ClientScriptManager reference from the Page class.
            ClientScriptManager cs = this.Page.ClientScript;

            // Check to see if the client script is already registered.
            if (!cs.IsClientScriptBlockRegistered(cstype, csTitle))
            {
                //Build the Script 
                StringBuilder csGradingClientScript = new StringBuilder(1000);

                csGradingClientScript.AppendLine("<!-- Place Holder Grading Client Script -->");

                csGradingClientScript.AppendLine("slk_selectedGradingRowId = null;");

                csGradingClientScript.AppendLine("var arrActualGradedPoints = new Array();");
                csGradingClientScript.AppendLine("var slk_arrOpenedFrameSets = new Array();");


                foreach (GradingItem item in this.Items)
                {
                    csGradingClientScript.Append(@"
                     arrActualGradedPoints[");
                    csGradingClientScript.Append(item.LearnerAssignmentId);
                    csGradingClientScript.Append(@"] = ");
                    if (item.GradedScore == null)
                    {
                        csGradingClientScript.Append("null");
                    }
                    else
                    {
                        csGradingClientScript.Append(item.GradedScore);
                    }
                    csGradingClientScript.Append(@";");
                }
                string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                csGradingClientScript.AppendLine(
                             String.Format(CultureInfo.InvariantCulture,
                                           "var slk_strFinalScorePrefix = \"{0}\";",
                                           FinalScoreId
                                           ));
                csGradingClientScript.AppendLine(
                            String.Format(CultureInfo.InvariantCulture,
                                          "var slk_strDecimalChar = \"{0}\";",
                                          decimalSeparator
                                         ));

                //Construct frameset  Handler JavaScript . 
                //This Method is invoked from Frameset. 
                //SlkUpdateLearnerAssignment(learnerAssignmentId,/* Id */ 
                //assignmentStatus /* as localized string */,
                //passFail /* as "passed" or "failed" or unknown */ , 
                //finalPoints, /*finalScore */
                //computedPoints /*Graded Score */ )
                //assignmentStatus and finalPoints values are not used in Grading. 
                csGradingClientScript.AppendLine(@"
                function SlkUpdateLearnerAssignment(learnerAssignmentId, assignmentStatus,
                                                    successStatus, finalPoints, computedPoints)
                {
                    
                    if((learnerAssignmentId == undefined) &&
                       (assignmentStatus == undefined)&&
                       (successStatus == undefined)&&
                       (finalPoints == undefined)&&
                       (computedPoints == undefined) 
                      )
                    {                        
                        return;
                    }
                "
                );
                csGradingClientScript.AppendLine(
                             String.Format(CultureInfo.InvariantCulture,
                                           "var slk_strGradedScorePrefix = \"{0}\";",
                                           GradingScoreId
                                          ));
                csGradingClientScript.AppendLine(
                             String.Format(CultureInfo.InvariantCulture,
                                           "var slk_strGradedImagePrefix = \"{0}\";",
                                           GradingImageId
                                          ));
                csGradingClientScript.AppendLine(
                            String.Format(CultureInfo.InvariantCulture,
                                          "var slk_strGradingUnSatisfiedSrc = \"{0}\";",
                                          Constants.ImagePath + Constants.GradingUnSatisfiedIcon
                                         ));
                csGradingClientScript.AppendLine(
                         String.Format(CultureInfo.InvariantCulture,
                                       "var slk_strGradingSatisfiedSrc = \"{0}\";",
                                       Constants.ImagePath + Constants.GradingSatisfiedIcon
                                      ));
                csGradingClientScript.AppendLine(
                        String.Format(CultureInfo.InvariantCulture,
                                      "var slk_strGradingUnKnownSrc = \"{0}\";",
                                      Constants.BlankGifUrl));

                csGradingClientScript.AppendLine(@"                    
                    var finalScoreId = slk_strFinalScorePrefix + learnerAssignmentId ;
                    if(document.getElementById(finalScoreId) == null && 
                        document.getElementById(finalScoreId) == undefined)
                    {
                        return;
                    }
                    document.getElementById(finalScoreId).disabled = false; 
                    if(Trim(learnerAssignmentId) == """")
                    {
                        return;
                    }
                    slk_arrOpenedFrameSets[learnerAssignmentId] = null;
                    var gradedPoints = Slk_ParseDouble(computedPoints);    
                    
                    var finalScore = Slk_GradingGetFinalScore(gradedPoints , learnerAssignmentId);
                    document.getElementById(finalScoreId).value 
                                        = finalScore == null ? """" : Slk_FormatDouble(finalScore);    
                    
                    var gradedScoreId = slk_strGradedScorePrefix + learnerAssignmentId ;
                                       
                    var gradedScoreText = gradedPoints == null ? """" : 
                                          Slk_FormatDouble(
                                                     Math.round(gradedPoints*Math.pow(10,2))/Math.pow(10,2));
                    document.getElementById(gradedScoreId).innerText = gradedScoreText == """"? gradedScoreText :
                                (gradedScoreText+""0"").match(/.*\.\d{2}/) || gradedScoreText + "".00"";
                 
                    document.getElementById(gradedScoreId).title 
                                        = gradedPoints == null ?"""": Slk_FormatDouble(gradedPoints) + "" "" + 
                                          document.getElementById(gradedScoreId).ToolTipText;

                    var gradedImageId = slk_strGradedImagePrefix + learnerAssignmentId ;
                 ");
                csGradingClientScript.Append(@"
                    if(successStatus == """);
                csGradingClientScript.Append(SuccessStatus.Passed.ToString());
                csGradingClientScript.Append(@""")
                    {
                        document.getElementById(gradedImageId).src = slk_strGradingSatisfiedSrc;
                    }
                    else if(successStatus == """);
                csGradingClientScript.Append(SuccessStatus.Failed.ToString());
                csGradingClientScript.Append(@""")
                    {
                        document.getElementById(gradedImageId).src = slk_strGradingUnSatisfiedSrc;
                    }
                    else if(successStatus == """);
                csGradingClientScript.Append(SuccessStatus.Unknown.ToString());
                csGradingClientScript.Append(@""")
                    {
                        document.getElementById(gradedImageId).src = slk_strGradingUnKnownSrc;
                    }   
                }                      
		        ");

                csGradingClientScript.AppendLine(@" 
                function Slk_GradingGetFinalScore(modifiedGradedPoints, learnerAssignmentId)
                {
                    var finalPointsElementId = slk_strFinalScorePrefix + learnerAssignmentId ;
                    var finalPoints = Slk_ParseDouble(document.getElementById(finalPointsElementId).value);
                    var actualGradedPoints = arrActualGradedPoints[learnerAssignmentId];
                                   
                    switch((((actualGradedPoints != null)<< 2) |
                            ((finalPoints != null) << 1)|
                            ((modifiedGradedPoints != null)))+ 1)
                    {
                        case 1:
                          break    
                        case 2:                          
                          finalPoints = modifiedGradedPoints;
                          break
                        case 3:
                          break    
                        case 4:
                             finalPoints = finalPoints + modifiedGradedPoints;
                          break
                        case 5:                         
                          break    
                        case 6:
                            var isOverride = confirm(""" +
                String.Format(CultureInfo.CurrentCulture,
                              AppResources.GradingConfirmOverrideFinalPoints,
                              "\"+ modifiedGradedPoints +\"") + @""");");
                csGradingClientScript.AppendLine(@"
                            if(isOverride)
                            {
                                finalPoints = modifiedGradedPoints;
                            }                             
                          break
                        case 7:
                          finalPoints = modifiedGradedPoints;
                          break    
                        case 8:                         
                          var deltaScore =  finalPoints - actualGradedPoints;
                          finalPoints = modifiedGradedPoints + deltaScore  ;
                          break
                        default:
                            finalPoints = null;
                    }
                    arrActualGradedPoints[learnerAssignmentId] = modifiedGradedPoints;
                    
                    return finalPoints;          
                 }");


                //Method for Highlight/UnHighlight the Selected Graded Row. 
                csGradingClientScript.AppendLine(@"             
                 function Slk_GradingHighlightGradingRow(rowId)
                 {
                  "
                );
                csGradingClientScript.Append(
                             String.Format(CultureInfo.InvariantCulture,
                                           "var gradingRowPrefix = \"{0}\";",
                                           GradingRowId
                                          ));
                csGradingClientScript.AppendLine(@"
                    var selectedGradingRowId = gradingRowPrefix + rowId;
                    obj = document.getElementById(selectedGradingRowId);
                    if(obj != null)
                    {
                      if (slk_selectedGradingRowId != null)
                      {
	                    document.getElementById(slk_selectedGradingRowId).className = """";
                      }
                      slk_selectedGradingRowId = obj.id;

                      if (slk_selectedGradingRowId != null)
                      {
	                    obj.className = ""ms-highlight"";
                      }
                    }
                }
                function Slk_GradingFinalScoreDisabled(obj)
                {  
                    if(obj != null && obj.disabled)
                    {
                        alert(obj.title);
                    }                       
                }    

                function Trim(s) 
                {
                   var m = s.match(/^\s*(\S+(\s+\S+)*)\s*$/);
                   return (m == null) ? """" : m[1];
                }

                function Slk_DisableGradingFinalScore(navigateUrl, 
                                                      learnerAssignmentId, 
                                                      isClassServerContent)
                {                   

                    var finalScoreElementId = slk_strFinalScorePrefix + learnerAssignmentId ;

                     if(document.getElementById(finalScoreElementId) != null && 
                        document.getElementById(finalScoreElementId) != undefined)
                    {
                        document.getElementById(finalScoreElementId).disabled = isClassServerContent; 
                    } 

                    if((slk_arrOpenedFrameSets[learnerAssignmentId] != undefined) &&
                       (slk_arrOpenedFrameSets[learnerAssignmentId] != null)&&
                       (!slk_arrOpenedFrameSets[learnerAssignmentId].closed) 
                      )
                    {
                        slk_arrOpenedFrameSets[learnerAssignmentId].focus();
                    } 
                    else
                    {
                        var openWindow = window.open(navigateUrl , ""_blank"");
                        slk_arrOpenedFrameSets[learnerAssignmentId] = openWindow;
                    }
                }       

                function Slk_GradingValidateFinalScore(obj)   
                {
                   if(obj != null)
                    { 
                        var finalScore = obj.value; 
                        if(Trim(finalScore) != """")
                        {                       
                            if(Slk_ParseDouble(finalScore) == null)
                            {");
                csGradingClientScript.Append(@"
                                alert(""");
                csGradingClientScript.Append(AppResources.GradingFinalScoreNaNError);
                csGradingClientScript.Append(@""");                         
                                obj.select();
                            }
                        }
                    }
                }         
                function Slk_ParseDouble(str)
                {                    
                    var re = new RegExp(""^\\s*([-\\+])?(\\d*)\\"" + slk_strDecimalChar + ""?(\\d*)\\s*$""); 
                    var m = str.match(re); 
                    if (m == null) 
                        return null; 
                    if (m[2].length == 0 && m[3].length == 0) 
                        return null; 
                    var strCleanInput = (m[1] != null ? m[1] : """") + (m[2].length > 0 ? m[2] : ""0"") +
                        (m[3].length > 0 ? ""."" + m[3] : """"); 
                    var dbl = parseFloat(strCleanInput); 
                    return (isNaN(dbl) ? null : dbl); 
                }
                
                function Slk_FormatDouble(str)
                { 
                  return String(str).replace(/\./g, slk_strDecimalChar); 
                }
                 "
            );
                csGradingClientScript.AppendLine("<!-- Grading Client Script Ends Here -->");

                //Register Learner/Learner Group onclick events as ClientScriptBlock
                cs.RegisterClientScriptBlock(cstype, csTitle, csGradingClientScript.ToString(), true);
            }

        }
        #endregion

        #endregion
    }

    /// <summary>
    /// Represents a data item in a Grading list control. 
    /// captures grades and comments  and other related information 
    /// for each learner assignment.
    /// This class cannot be inherited.  
    /// </summary>
    internal sealed class GradingItem
    {

        #region Private Variables
        /// <summary>
        /// Holds Assignment ID
        /// </summary> 
        private long m_learnerAssignmentId;
        /// <summary>
        /// Holds Learner�s name
        /// </summary> 
        private string m_learnerName;
        /// <summary>
        /// Holds the status field
        /// </summary>
        private LearnerAssignmentState m_status;
        /// <summary>
        /// Holds the results
        /// </summary>
        private SuccessStatus m_successStatus;
        /// <summary>
        /// Holds the computed points.
        /// </summary>    
        private float? m_gradedScore;
        /// <summary>
        /// Holds computed points value
        /// </summary>
        private float? m_finalScore;
        /// <summary>
        /// Holds InstructorComments Value
        /// </summary>
        private string m_instructorComments;
        /// <summary>
        /// Holds Action Text
        /// </summary>
        private string m_actionText;
        /// <summary>
        /// Holds Action State
        /// </summary>
        private bool m_actionState;


        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>

        public GradingItem()
        {

        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Learner assignment ID
        /// </summary> 
        internal long LearnerAssignmentId
        {
            get { return m_learnerAssignmentId; }
            set { m_learnerAssignmentId = value; }
        }
        /// <summary>
        /// Learner�s name
        /// </summary> 
        internal string LearnerName
        {
            get { return m_learnerName; }
            set { m_learnerName = value; }
        }
        /// <summary>
        /// Displays the status of the learner assignment
        /// </summary>
        internal LearnerAssignmentState Status
        {
            get { return m_status; }
            set { m_status = value; }
        }
        /// <summary>
        /// Displays the SuccessStatus of the learner assignment
        /// </summary>
        internal SuccessStatus SuccessStatus
        {
            get { return m_successStatus; }
            set { m_successStatus = value; }
        }
        /// <summary>
        /// Shows the computed points rouded to the nearest integer.
        /// </summary>
        internal float? GradedScore
        {
            get { return m_gradedScore; }
            set { m_gradedScore = value; }
        }

        /// <summary>
        /// computed points value with full precision
        /// </summary>
        internal float? FinalScore
        {
            get { return m_finalScore; }
            set { m_finalScore = value; }
        }
        /// <summary>
        /// InstructorComments
        /// </summary>
        internal string InstructorComments
        {
            get { return m_instructorComments; }
            set { m_instructorComments = value; }
        }
        /// <summary>
        /// Action State
        /// </summary>
        internal string ActionText
        {
            get { return m_actionText; }
            set { m_actionText = value; }
        }

        /// <summary>
        /// Action Checked
        /// </summary>
        internal bool ActionState
        {
            get { return m_actionState; }
            set { m_actionState = value; }
        }

        #endregion
    }

    /// <summary>
    /// A collection of GradingItem objects in a GradingListcontrol. 
    /// This class cannot be inherited. 
    /// </summary>
    internal sealed class GradingItemCollection : List<GradingItem>, IStateManager
    {
        private bool m_isTracked;

        #region Private and Public Methods
        /// <summary>
        /// Searches the collection for a GradingItem with a 
        /// Learner Assignment Id as Key Value. 
        /// </summary>
        /// <param name="value">Learner Assignment Id</param>
        /// <returns>GradingItem</returns>
        internal GradingItem FindByValue(long value)
        {
            int num = 0;
            foreach (GradingItem item1 in this)
            {
                if (item1.LearnerAssignmentId == value)
                {
                    return this[num];
                }
                num++;
            }
            return null;
        }
        #endregion

        #region IStateManager Members
        /// <summary>
        /// TrackingViewState
        /// </summary>
        public bool IsTrackingViewState
        {
            get { return m_isTracked; }
        }

        #region LoadViewState
        /// <summary>
        /// LoadsViewState
        /// </summary>
        /// <param name="state">savedState</param>
        public void LoadViewState(object state)
        {
            if (state != null)
            {
                Triplet obj = (Triplet)state;
                Clear();
                Pair objState1 = (Pair)obj.First;
                Triplet objState2 = (Triplet)obj.Second;
                Triplet objState3 = (Triplet)obj.Third;

                long[] assignmentId = (long[])objState1.First;
                string[] learnerName = (string[])objState1.Second;
                LearnerAssignmentState[] status = (LearnerAssignmentState[])objState2.First;
                SuccessStatus[] successStatus = (SuccessStatus[])objState2.Second;
                string[] gradedScore = (string[])objState2.Third;
                string[] finalScore = (string[])objState3.First;
                string[] instructorComments = (string[])objState3.Second;
                bool[] actionState = (bool[])objState3.Third;

                for (int i = 0; i < assignmentId.Length; i++)
                {
                    GradingItem item = new GradingItem();
                    item.LearnerAssignmentId = assignmentId[i];
                    item.LearnerName = learnerName[i];
                    item.Status = status[i];
                    item.SuccessStatus = successStatus[i];
                    if (!String.IsNullOrEmpty(gradedScore[i]))
                        item.GradedScore = float.Parse(gradedScore[i],
                                                       CultureInfo.CurrentCulture.NumberFormat);
                    if (!String.IsNullOrEmpty(finalScore[i]))
                        item.FinalScore = float.Parse(finalScore[i],
                                                      CultureInfo.CurrentCulture.NumberFormat);
                    item.InstructorComments = instructorComments[i];
                    item.ActionState = actionState[i];
                    Add(item);
                }
            }

        }
        #endregion

        #region SaveViewState
        /// <summary>
        /// SaveViewState
        /// </summary>
        /// <returns>savedObject</returns>
        public object SaveViewState()
        {
            if (Count > 0)
            {
                int numOfItems = Count;

                long[] assignmentId = new long[numOfItems];
                string[] learnerName = new string[numOfItems];
                LearnerAssignmentState[] learnerStatus = new LearnerAssignmentState[numOfItems];
                SuccessStatus[] successStatus = new SuccessStatus[numOfItems];
                string[] gradedScore = new string[numOfItems];
                string[] finalScore = new string[numOfItems];
                string[] instructorComments = new string[numOfItems];
                bool[] actionState = new bool[numOfItems];

                for (int i = 0; i < numOfItems; i++)
                {
                    assignmentId[i] = this[i].LearnerAssignmentId;
                    learnerName[i] = this[i].LearnerName;
                    learnerStatus[i] = this[i].Status;
                    successStatus[i] = this[i].SuccessStatus;
                    gradedScore[i]
                            = this[i].GradedScore == null ?
                                                String.Empty :
                                                this[i].GradedScore.Value.ToString(CultureInfo.CurrentCulture);
                    finalScore[i]
                        = this[i].FinalScore == null ?
                                            String.Empty :
                                            this[i].FinalScore.Value.ToString(CultureInfo.CurrentCulture);
                    instructorComments[i] = this[i].InstructorComments;
                    actionState[i] = this[i].ActionState;
                }

                Pair objState1 = new Pair(assignmentId, learnerName);
                Triplet objState2 = new Triplet(learnerStatus, successStatus, gradedScore);
                Triplet objState3 = new Triplet(finalScore, instructorComments, actionState);

                return new Triplet(objState1, objState2, objState3);

            }

            return null;

        }
        #endregion

        #region TrackViewState
        /// <summary>
        /// TrackViewState
        /// </summary>
        public void TrackViewState()
        {
            m_isTracked = true;
        }
        #endregion

        #endregion
    }



}