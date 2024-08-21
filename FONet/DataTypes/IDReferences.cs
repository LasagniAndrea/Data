namespace Fonet.DataTypes
{
    using System.Collections;
    using System.Text;
    using Fonet.Layout;
    using Fonet.Pdf;

    internal class IDReferences
    {
        private readonly Hashtable idReferences;
        private readonly Hashtable idValidation;
        private readonly Hashtable idUnvalidated;
        private const int ID_PADDING = 5000;

        public IDReferences()
        {
            idReferences = new Hashtable();
            idValidation = new Hashtable();
            idUnvalidated = new Hashtable();
        }

        public void InitializeID(string id, Area area)
        {
            CreateID(id);
            ConfigureID(id, area);
        }

        public void CreateID(string id)
        {
            if (id != null && !id.Equals(""))
            {
                if (DoesUnvalidatedIDExist(id))
                {
                    RemoveFromUnvalidatedIDList(id);
                    RemoveFromIdValidationList(id);
                }
                else if (DoesIDExist(id))
                {
                    throw new FonetException("The id \"" + id
                        + "\" already exists in this document");
                }
                else
                {
                    CreateNewId(id);
                    RemoveFromIdValidationList(id);
                }

            }
        }

        public void CreateUnvalidatedID(string id)
        {
            if (id != null && !id.Equals(""))
            {
                if (!DoesIDExist(id))
                {
                    CreateNewId(id);
                    AddToUnvalidatedIdList(id);
                }
            }
        }

        public void AddToUnvalidatedIdList(string id)
        {
            idUnvalidated[id] = "";
        }

        public void RemoveFromUnvalidatedIDList(string id)
        {
            idUnvalidated.Remove(id);
        }

        public bool DoesUnvalidatedIDExist(string id)
        {
            return idUnvalidated.ContainsKey(id);
        }

        public void ConfigureID(string id, Area area)
        {
            if (id != null && !id.Equals(""))
            {
                SetPosition(id,
                            area.GetPage().GetBody().GetXPosition()
                                + area.GetTableCellXOffset() - ID_PADDING,
                            area.GetPage().GetBody().GetYPosition()
                                - area.GetAbsoluteHeight() + ID_PADDING);
                SetPageNumber(id, area.GetPage().GetNumber());
                area.GetPage().AddToIDList(id);
            }
        }

        public void AddToIdValidationList(string id)
        {
            idValidation[id] = "";
        }

        public void RemoveFromIdValidationList(string id)
        {
            idValidation.Remove(id);
        }

        public void RemoveID(string id)
        {
            idReferences.Remove(id);
        }

        public bool IsEveryIdValid()
        {
            return (idValidation.Count == 0);
        }

        public string GetInvalidIds()
        {
            StringBuilder list = new StringBuilder();
            foreach (object o in idValidation.Keys)
            {
                list.Append("\n\"");
                list.Append(o.ToString());
                list.Append("\" ");
            }
            return list.ToString();
        }

        public bool DoesIDExist(string id)
        {
            return idReferences.ContainsKey(id);
        }

        public bool DoesGoToReferenceExist(string id)
        {
            IDNode node = (IDNode)idReferences[id];
            return node.IsThereInternalLinkGoTo();
        }

        public PdfGoTo GetInternalLinkGoTo(string id)
        {
            IDNode node = (IDNode)idReferences[id];
            return node.GetInternalLinkGoTo();
        }

        public PdfGoTo CreateInternalLinkGoTo(string id, PdfObjectId objectId)
        {
            IDNode node = (IDNode)idReferences[id];
            node.CreateInternalLinkGoTo(objectId);
            return node.GetInternalLinkGoTo();
        }

        public void CreateNewId(string id)
        {
            IDNode node = new IDNode(id);
            idReferences[id] = node;
        }

        public PdfGoTo GetPDFGoTo(string id)
        {
            IDNode node = (IDNode)idReferences[id];
            return node.GetInternalLinkGoTo();
        }

        public void SetInternalGoToPageReference(string id,
                                                 PdfObjectReference pageReference)
        {
            IDNode node = (IDNode)idReferences[id];
            if (node != null)
            {
                node.SetInternalLinkGoToPageReference(pageReference);
            }
        }

        public void SetPageNumber(string id, int pageNumber)
        {
            IDNode node = (IDNode)idReferences[id];
            node.SetPageNumber(pageNumber);
        }

        public string GetPageNumber(string id)
        {
            if (DoesIDExist(id))
            {
                IDNode node = (IDNode)idReferences[id];
                return node.GetPageNumber();
            }
            else
            {
                AddToIdValidationList(id);
                return null;
            }
        }

        public void SetPosition(string id, int x, int y)
        {
            IDNode node = (IDNode)idReferences[id];
            node.SetPosition(x, y);
        }

        public ICollection GetInvalidElements()
        {
            return idValidation.Keys;
        }
    }
}