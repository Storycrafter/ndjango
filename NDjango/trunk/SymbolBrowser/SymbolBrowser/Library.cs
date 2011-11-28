﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SymbolBrowser.ObjectLists;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.SymbolBrowser
{
    public class Library : IVsSimpleLibrary2
    {
        #region IVsSimpleLibrary2 Members

        public int AddBrowseContainer(VSCOMPONENTSELECTORDATA[] pcdComponent, ref uint pgrfOptions, out string pbstrComponentAdded)
        {
            throw new NotImplementedException();
        }

        public int CreateNavInfo(SYMBOL_DESCRIPTION_NODE[] rgSymbolNodes, uint ulcNodes, out IVsNavInfo ppNavInfo)
        {
            throw new NotImplementedException();
        }

        public int GetBrowseContainersForHierarchy(IVsHierarchy pHierarchy, uint celt, VSBROWSECONTAINER[] rgBrowseContainers, uint[] pcActual = null)
        {
            throw new NotImplementedException();
        }

        public int GetGuid(out Guid pguidLib)
        {
            pguidLib = GetType().GUID;
            return VSConstants.S_OK;
        }

        public int GetLibFlags2(out uint pgrfFlags)
        {
            pgrfFlags = (uint)_LIB_FLAGS.LF_PROJECT | (uint)_LIB_FLAGS2.LF_SUPPORTSLISTREFERENCES;
            return VSConstants.S_OK;
        }

        public int GetList2(uint ListType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2)
        {/*
            string strSearchCriteria = pobSrch[0].szName;
            uint grfOptions = pobSrch[0].grfOptions;
            IVsNavInfo NavInfo = pobSrch[0].pIVsNavInfo;

            // Return generated list of symbols to the object manager.
            ResultList resultsList = new RootMethodsList(this, grfOptions);
            ppIVsSimpleObjectList2 = (Microsoft.VisualStudio.Shell.Interop.IVsSimpleObjectList2)(resultsList);
            string strFullNameFromNavInfo;

            if (((uint)(grfOptions & (uint)Microsoft.VisualStudio.Shell.Interop._VSOBSEARCHOPTIONS2.VSOBSO_CALLSFROM) > 0) &&
                ((uint)(grfOptions & (uint)Microsoft.VisualStudio.Shell.Interop._VSOBSEARCHOPTIONS2.VSOBSO_CALLSTO) > 0))
            {
                // Initial view with VSOBSO_CALLSFROM and VSOBSO_CALLSTO flags set simultaneously.
                // Generate list of all methods in the container.

                foreach (CallInstance call in m_CallGraph)
                {
                    resultsList.AddMethod(call.m_Source);
                    resultsList.AddMethod(call.m_Target);
                }

            }
            // Generate CALLFROM list for Call graph.
            else if ((uint)(grfOptions & (uint)Microsoft.VisualStudio.Shell.Interop._VSOBSEARCHOPTIONS2.VSOBSO_CALLSFROM) > 0)
            {
                System.Collections.Generic.List<CallInstance> Calls = null;

                if (NavInfo != null)
                {
                    strFullNameFromNavInfo = CallBrowserNavInfo.GetFullNameFromNavInfo(NavInfo);
                    Calls = m_CallGraph.GetCallGraph(strFullNameFromNavInfo);
                }
                else if (strSearchCriteria.Length > 0)
                {
                    Calls = m_CallGraph.GetCallGraph(strSearchCriteria);
                }

                for (int i = 0; i < Calls.Count; i++)
                {
                    Method method = Calls[i].m_Source;
                    resultsList.AddMethod(method);
                }
            }
            // Generate CALLTO list for Callers graph.

            else if ((uint)(grfOptions & (uint)Microsoft.VisualStudio.Shell.Interop._VSOBSEARCHOPTIONS2.VSOBSO_CALLSTO) > 0)
            {
                System.Collections.Generic.List<CallInstance> Callers = null; // = new System.Collections.Generic.List<CallInstance>();

                if (NavInfo != null)
                {
                    strFullNameFromNavInfo = CallBrowserNavInfo.GetFullNameFromNavInfo(NavInfo);
                    Callers = m_CallGraph.GetCallersGraph(strFullNameFromNavInfo);
                }
                else if (strSearchCriteria.Length > 0)
                {
                    Callers = m_CallGraph.GetCallersGraph(strSearchCriteria);
                }

                for (int i = 0; i < Callers.Count; i++)
                {
                    Method method = Callers[i].m_Target;
                    resultsList.AddMethod(method);
                }
            }

            return VSConstants.S_OK;
            */
            
            switch (ListType)
            {
                case (uint)_LIB_LISTTYPE.LLT_PHYSICALCONTAINERS: //16
                    ppIVsSimpleObjectList2 = null;
                    return VSConstants.E_FAIL;
                case (uint)_LIB_LISTTYPE.LLT_NAMESPACES: //2
                    ppIVsSimpleObjectList2 = null;
                    return VSConstants.E_FAIL;
                case (uint)_LIB_LISTTYPE.LLT_CLASSES: //??
                    ppIVsSimpleObjectList2 = null;
                    return VSConstants.E_FAIL;
                case (uint)_LIB_LISTTYPE.LLT_MEMBERS: //8
                    ppIVsSimpleObjectList2 = null;
                    return VSConstants.E_FAIL;
                case (uint)_LIB_LISTTYPE.LLT_REFERENCES: //8192
                    ppIVsSimpleObjectList2 = null;
                    return VSConstants.E_FAIL;

                default:
                    ppIVsSimpleObjectList2 = null;
                    return VSConstants.E_FAIL;
            }
        }

        public int GetSeparatorStringWithOwnership(out string pbstrSeparator)
        {
            pbstrSeparator = ".";
            return VSConstants.S_OK;
        }

        public int GetSupportedCategoryFields2(int Category, out uint pgrfCatField)
        {
            /*switch (Category)
            {
                case (int)LIB_CATEGORY.LC_MEMBERTYPE:
                    pgrfCatField = (uint)_LIBCAT_MEMBERTYPE.LCMT_METHOD;
                    break;

                case (int)LIB_CATEGORY.LC_MEMBERACCESS:
                    pgrfCatField = (uint)_LIBCAT_MEMBERACCESS.LCMA_PUBLIC |
                                (uint)_LIBCAT_MEMBERACCESS.LCMA_PRIVATE |
                                (uint)_LIBCAT_MEMBERACCESS.LCMA_PROTECTED |
                                (uint)_LIBCAT_MEMBERACCESS.LCMA_PACKAGE |
                                (uint)_LIBCAT_MEMBERACCESS.LCMA_FRIEND |
                                (uint)_LIBCAT_MEMBERACCESS.LCMA_SEALED;
                    break;

                case (int)LIB_CATEGORY.LC_LISTTYPE:
                    pgrfCatField = (uint)_LIB_LISTTYPE.LLT_MEMBERS;
                    break;

                case (int)LIB_CATEGORY.LC_VISIBILITY:
                    pgrfCatField = (uint)(_LIBCAT_VISIBILITY.LCV_VISIBLE |
                                        _LIBCAT_VISIBILITY.LCV_HIDDEN);
                    break;

                default:
                    pgrfCatField = 0;
                    return VSConstants.E_FAIL;
            }
            return VSConstants.S_OK;
            */
            
            switch (Category)
            {
                case (int)LIB_CATEGORY.LC_LISTTYPE:
                    pgrfCatField = (uint)_LIB_LISTTYPE.LLT_REFERENCES;
                    return VSConstants.S_OK;
                case (int)LIB_CATEGORY.LC_ACTIVEPROJECT:
                case (int)LIB_CATEGORY.LC_CLASSACCESS:
                case (int)LIB_CATEGORY.LC_CLASSTYPE:
                case (int)LIB_CATEGORY.LC_MEMBERACCESS:
                case (int)LIB_CATEGORY.LC_MEMBERTYPE:
                case (int)LIB_CATEGORY.LC_MODIFIER:
                case (int)LIB_CATEGORY.LC_NODETYPE:
                case (int)LIB_CATEGORY.LC_VISIBILITY:
                case (int)_LIB_CATEGORY2.LC_HIERARCHYTYPE:
                case (int)_LIB_CATEGORY2.LC_MEMBERINHERITANCE:
                case (int)_LIB_CATEGORY2.LC_NIL:
                case (int)_LIB_CATEGORY2.LC_PHYSICALCONTAINERTYPE:
                case (int)_LIB_CATEGORY2.LC_SEARCHMATCHTYPE:
                default:
                    pgrfCatField = 0;
                    return VSConstants.E_FAIL;
            }
        }

        public int LoadState(VisualStudio.OLE.Interop.IStream pIStream, LIB_PERSISTTYPE lptType)
        {
            throw new NotImplementedException();
        }

        public int RemoveBrowseContainer(uint dwReserved, string pszLibName)
        {
            throw new NotImplementedException();
        }

        public int SaveState(VisualStudio.OLE.Interop.IStream pIStream, LIB_PERSISTTYPE lptType)
        {
            throw new NotImplementedException();
        }

        private uint updateCounter;
        public int UpdateCounter(out uint pCurUpdate)
        {
            pCurUpdate = updateCounter;
            return VSConstants.S_OK;
        }

        #endregion
    }
}