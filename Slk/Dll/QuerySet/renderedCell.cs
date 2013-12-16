using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.SharePoint;
using Microsoft.LearningComponents;
using Microsoft.LearningComponents.Storage;
using Schema = Microsoft.LearningComponents.Storage.BaseSchema;
using Resources.Properties;

namespace Microsoft.SharePointLearningKit
{
    /// <summary>
    /// Represents the data in one column of one row of the results of a query generated by
    /// <c>QueryDefinition.CreateQuery</c>.  <c>RenderedCell</c> values are generated by
    /// <c>QueryDefinition.RenderQueryRowCells</c>.  <c>RenderedCell</c> values are generated by
    /// </summary>
    ///
    public class RenderedCell
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Private Fields
        //

        /// <summary>
        /// Holds the value of the <c>Value</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object m_value;

        /// <summary>
        /// Holds the value of the <c>SortKey</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object m_sortKey;

        /// <summary>
        /// Holds the value of the <c>Id</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        LearningStoreItemIdentifier m_id;

        /// <summary>
        /// Holds the value of the <c>ToolTip</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string m_toolTip;

        /// <summary>
        /// Holds the value of the <c>Wrap</c> property.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool m_wrap;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Public Properties
        //

        /// <summary>
        /// Gets the rendered value of the cell.  In some cases, such as <c>ColumnRenderAs.Default</c>
        /// rendering with no <c>ColumnDefinition.CellFormat</c>, this is the original value returned
        /// from the query.  In other cases this is a formatted string.
        /// </summary>
        public object Value
        {
            [DebuggerStepThrough]
            get
            {
                return m_value;
            }
        }

        /// <summary>
        /// Gets the sort key of the cell.  When sorting cells, compare <c>SortKey</c> values.
        /// </summary>
        public object SortKey
        {
            [DebuggerStepThrough]
            get
            {
                return m_sortKey;
            }
        }

        /// <summary>
        /// Gets the <c>LearningStoreItemIdentifier</c>, if any, associated with the cell, or
        /// <c>null</c> if none.
        /// </summary>
        public LearningStoreItemIdentifier Id
        {
            [DebuggerStepThrough]
            get
            {
                return m_id;
            }
        }

        /// <summary>
        /// Gets the tooltip text, if any, associated with the cell, or <c>null</c> if none.
        /// </summary>
        public string ToolTip
        {
            [DebuggerStepThrough]
            get
            {
                return m_toolTip;
            }
        }

        /// <summary>
        /// The value of the "Wrap" attribute: <c>true</c> if this rendered cell can wrap to the next
        /// line, <c>false</c> if not.
        /// </summary>
        public bool Wrap
        {
            [DebuggerStepThrough]
            get
            {
                return m_wrap;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Public Methods
        //

        /// <summary>
        /// Returns a string representation of this object -- the same as <r>Value</r>, except
        /// <r>ToString</r> returns <n>String.Empty</n> instead of <n>null</n>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (m_value == null)
                return String.Empty;
            else
                return m_value.ToString();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Internal Methods
        //

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        ///
        /// <param name="value">The value of the <c>Value</c> property.</param>
        ///
        /// <param name="sortKey">The value of the <c>SortKey</c> property.</param>
        ///
        /// <param name="id">The value of the <c>Id</c> property.</param>
        ///
        /// <param name="toolTip">The value of the <c>ToolTip</c> property.</param>
        ///
        /// <param name="wrap">The value of the <c>Wrap</c> property.</param>
        ///
        internal RenderedCell(object value, object sortKey, LearningStoreItemIdentifier id,
            string toolTip, bool wrap)
        {
            m_value = value;
            m_sortKey = sortKey;
            m_id = id;
            m_toolTip = toolTip;
            m_wrap = wrap;
        }
    }
}
