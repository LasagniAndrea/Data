using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Spheres.DataContracts;

namespace EFS.Spheres.Hierarchies
{

    /// <summary>
    /// Delegate method checking a node property
    /// </summary>
    /// <param name="pParents">list of the parents of the current node</param>
    /// <param name="pCurrentNode">current node</param>
    /// <param name="opProperty">output flag reflecting some node property</param>
    /// <param name="opActorId">output actor node id reflecting some node property</param>
    /// <param name="opError">error string, not null just in case the returned value is false</param>
    /// <typeparam name="TNode">type of the node object</typeparam>
    /// <returns>true when the checked node is good</returns>
    
    //public delegate bool GetNodeProperty<TNode>(
    //    List<TNode> pParents, TNode pCurrentNode,
    //        out bool opProperty, out int opActorId, out string opError);
    public delegate bool GetNodeProperty<TNode>(
        List<TNode> pParents, TNode pCurrentNode,
            out bool opProperty, out int opActorId, out SysMsgCode opError);

    /// <summary>
    /// Delegate method implementing a factory pattern, which instantiates attribute-objects 
    /// type ISpheresNodeAttribute
    /// </summary>
    /// <param name="pConnection">an open connection</param>
    /// <param name="pChildRelation">the actor relationship base of the returned attribute</param>
    /// <typeparam name="TRelation">type of the object used to identify the returned ATTRIBUTE type</typeparam>
    /// <returns>a new attribute</returns>
    public delegate ISpheresNodeAttribute GetAttributeInstance<TRelation>(
        IDbConnection pConnection, TRelation pChildRelation);

    /// <summary>
    /// Delegate method implementing a factory pattern, which instantiates node-objects 
    /// type ISpheresNode
    /// </summary>
    /// <param name="pConnection">an open connection</param>
    /// <param name="pChildRelation">the relationship base of the returned node</param>
    /// <typeparam name="TNode">BASE type of the returned node</typeparam>
    /// <typeparam name="TRelation">type of the object used to identify the returned NODE type</typeparam>
    /// <returns>a new node</returns>
    public delegate ISpheresNode<TNode, TRelation> GetNodeInstance<TNode, TRelation>(IDbConnection pConnection, TRelation pChildRelation);

    /// <summary>
    /// Interface describing a hierarchy tree
    /// </summary>
    /// <typeparam name="T">Class type of the NODE element of the hierarchy</typeparam>
    public interface ISpheresHierarchy<T, TRelation> : ICloneable
    {
        /// <summary>
        /// Get if the hierarchy has been built
        /// </summary>
        bool Built { get; }

        /// <summary>
        /// Get the root of the hierarchy
        /// </summary>
        T Root { get; }

        /// <summary>
        /// get the name of the CLR root type
        /// </summary>
        string RootType { get; }

        /// <summary>
        /// get the name of the CLR node type
        /// </summary>
        string NodeType { get; }

        /// <summary>
        /// Build the hierarchy starting from the given internal actor id
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pId">node id</param>
        /// <param name="pSessionId">session id</param>
        /// <returns></returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        bool BuildHierarchy(string pCS, int pId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSetErrorWarning"></param>
        /// FI 20200623 [XXXXX] Add SetErrorWarning
        void InitDelegates(SetErrorWarning pSetErrorWarning);

        /// <summary>
        /// INit the node factory
        /// </summary>
        /// <param name="pDelegate"></param>
        void InitNodeFactoryDelegate(GetNodeInstance<T, TRelation> pDelegate);

        /// <summary>
        /// Init the attribute factory
        /// </summary>
        /// <param name="pDelegate"></param>
        void InitAttributeFactoryDelegate(GetAttributeInstance<TRelation> pDelegate);

    }

    /// <summary>
    /// Interface describing a tree (type ISpheresHierarchy) node
    /// </summary>
    /// <typeparam name="T">Class type of the node itself</typeparam>
    public interface ISpheresNode<T, TRelation> : ICloneable
    {
        /// <summary>
        /// Get if the node has been built
        /// </summary>
        bool Built { get; }

        /// <summary>
        /// Node identifier
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Node internal id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Node display name
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Get the childs of the node
        /// </summary>
        List<T> ChildNodes { get; }

        /// <summary>
        /// Get the reference at a child inside of the childs collection (<seealso cref="ChildNodes"/>), having the given identifier
        /// </summary>
        /// <param name="pIdentifier">the node identifier</param>
        /// <returns>the child node if it exists, else null</returns>
        ISpheresNode<T, TRelation> GetChild(string pIdentifier);

        /// <summary>
        /// Build the Node
        /// </summary>
        /// <param name="pParents"></param>
        /// <param name="pConnection"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        bool BuildNode(List<ISpheresNode<T, TRelation>> pParents, IDbConnection pConnection);

        /// <summary>
        /// reset the node contents
        /// </summary>
        void ResetNode();

        /// <summary>
        /// Init the log method
        /// </summary>
        void InitDelegate(SetErrorWarning pSetErrorWarning);

        /// <summary>
        /// INit the node factory
        /// </summary>
        /// <param name="pDelegate">the delegate method </param>
        void InitNodeFactoryDelegate(GetNodeInstance<T, TRelation> pDelegate);

        /// <summary>
        /// init the attribute factory
        /// </summary>
        /// <param name="pDelegate">the delegate method</param>
        void InitAttributeFactoryDelegate(GetAttributeInstance<TRelation> pDelegate);

        /// <summary>
        /// Find recursively all the tree childs that match the predicate
        /// </summary>
        /// <param name="searchingPred">match predicate</param>
        /// <returns>a node list</returns>
        List<T> FindChilds(Predicate<T> searchingPred);

        /// <summary>
        /// Find all the ancestors of the current node that match the predicate
        /// </summary>
        /// <param name="pSearchRoot">actor node upper limit of the parent research</param>
        /// <param name="searchingPred">node specific properties to filter the ancestor nodes</param>
        /// <returns></returns>
        List<T> FindAncestors(T pSearchRoot, Predicate<T> searchingPred);
    }

    /// <summary>
    /// Interface describing a relation between the current node and another node of the tree
    /// </summary>
    public interface ISpheresNodeAttribute : ICloneable
    {
        /// <summary>
        /// Node internal id
        /// </summary>
        int IdNode { get; }

        /// <summary>
        /// Parent node internal id, it is the parent id of the relation
        /// </summary>
        int IdParentNode { get; }

        /// <summary>
        /// Get if the attribute has been built
        /// </summary>
        bool Built { get; }

        /// <summary>
        /// Build the attribute
        /// </summary>
        /// <returns></returns>
        bool BuildAttribute(IDbConnection pConnection);

        /// <summary>
        /// reset the attribute contents
        /// </summary>
        void ResetAttribute();

        
    }

    /// <summary>
    /// Interface describing a tree node without childs 
    /// </summary>
    public interface ISpheresNodeLeaf
    {

        /// <summary>
        /// Node identifier
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Node internal id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Node display name
        /// </summary>
        string DisplayName { get; }
    }

    public static class RolesCollection
    {
        /// <summary>
        /// the marging req office role ID
        /// </summary>
        public const string ROLEMARGINREQOFFICE = "MARGINREQOFFICE";

        /// <summary>
        /// The clearing house role ID
        /// </summary>
        public const string ROLECSS = "CSS";

        /// <summary>
        /// The clearer role ID, must be treated as a CSS
        /// </summary>
        public const string ROLECLEARER = "CLEARER";

        /// <summary>
        /// The entity role ID
        /// </summary>
        public const string ROLEENTITY = "ENTITY";
    }

    /// <summary>
    /// A class representing an actor/role Spheres hierarchy
    /// </summary>
    [XmlRoot(ElementName = "HierarchyTree")]
    public class ActorRoleHierarchy : ISpheresHierarchy<ActorNode, ActorRelationship>
    {

        ActorNode m_Root = null;

        bool m_Built = false;

        string m_RootType = typeof(ActorRoleHierarchy).FullName;

        string m_NodeType = typeof(ActorNode).FullName;


        // FI 20200623 [XXXXX] Add
        SetErrorWarning m_SetErrorWarning;

        GetNodeInstance<ActorNode, ActorRelationship> m_NodeFactory;

        GetAttributeInstance<ActorRelationship> m_AttributeFactory;

        /// <summary>
        /// Return an empty tree
        /// </summary>
        public ActorRoleHierarchy()
        {
        }

        /// <summary>
        /// Load the hierarchy starting from the given actor identifier
        /// </summary>
        /// <param name="pConnection">current open connection</param>
        /// <param name="pSessionId">session id</param>
        /// <param name="pId">internal actor id</param>
        /// <param name="pIdentifier">actor identifier </param>
        /// <param name="pDisplayName">actor display name, can be null</param>
        /// <returns></returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        // EG 20190114 ProcessLog delegate Refactoring
        public bool BuildHierarchy(IDbConnection pConnection, int pId, string pIdentifier, string pDisplayName)
        {
            if (pId <= 0)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, new ArgumentException("SYS-01016", "pId"));
            }

            this.Built = false;

            this.Root = (ActorNode)m_NodeFactory.Invoke(
                pConnection,
                new ActorRelationship
                {
                    RoleOwner = pIdentifier,
                    RoleOwnerId = pId,
                    RoleOwnerName = pDisplayName
                });

            //this.Root = new ActorNode(pIdentifier, pId, pDisplayName);

            this.Root.InitDelegate(m_SetErrorWarning);

            this.Root.InitNodeFactoryDelegate(m_NodeFactory);

            this.Root.InitAttributeFactoryDelegate(m_AttributeFactory);

            this.Built = this.Root.BuildNode(new List<ISpheresNode<ActorNode, ActorRelationship>>(), pConnection);

            return this.Built;
        }

        #region ISpheresHierarchy Membres

        /// <summary>
        /// Get the current state
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public bool Built
        {
            get
            {
                return m_Built;
            }

            set
            {
                m_Built = false;
            }
        }

        /// <summary>
        /// Return the root reference of the hierarchy
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public ActorNode Root
        {
            get
            {
                return m_Root;
            }

            set
            {
                m_Root = value;
            }
        }

        /// <summary>
        /// ?
        /// </summary>
        [XmlAttribute(AttributeName = "type")]
        [ReadOnly(true)]
        [Browsable(false)]
        public string RootType
        {
            get
            {
                return m_RootType;
            }

            set
            {
                m_RootType = value;
            }
        }

        /// <summary>
        /// ?
        /// </summary>
        [XmlAttribute(AttributeName = "nodetype")]
        [ReadOnly(true)]
        [Browsable(false)]
        public string NodeType
        {
            get
            {
                return m_NodeType;
            }

            set
            {
                m_NodeType = value;
            }
        }

        /// <summary>
        ///  Load the hierarchy starting from the given internal actor id
        /// </summary>
        /// <param name="pCS">the connection string</param>
        /// <param name="pId">a valid actor id</param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        public bool BuildHierarchy(string pCS, int pId)
        {
            if (pId <= 0)
            {
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, new ArgumentException("SYS-01016", "pId"));
            }

            SQL_Actor actorRoot = new SQL_Actor(pCS, pId);

            this.Built = false;

            IDbConnection connection = null;

            try
            {
                connection = DataHelper.OpenConnection(pCS);

                if (actorRoot.IsLoaded)
                    this.Built = BuildHierarchy(connection, actorRoot.Id, actorRoot.Identifier, actorRoot.DisplayName);
            }
            finally
            {
                if (connection != null)
                    DataHelper.CloseConnection(connection);
            }

            return this.Built;
        }

        /// <summary>
        /// Alimenter la table IMACTORPOS avec tous les acteurs:
        /// <para>1- avec des trades en position</para>
        /// <para>2- avec des tardes échus en attente de livraison</para>
        /// <para>La table IMACTORPOS est utilisée en jointure pour le chargement des hiérarchies d'acteurs:</para>
        /// <para>- Entity: méthode BuildEntityHierarchy()</para>
        /// <para>- Clearer: méthode BuildClearerHierarchies()</para>
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIdA_CssCustodian"></param>
        /// EG 20140221 [19575][19666] Ajout paramètre IDA_CSS pour critère de filtre sur IMACTORPOS 
        /// EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        /// EG 20181108 PERF Add parameter pSvrInfoConnection for Hints
        /// EG 20181119 PERF Correction post RC
        /// EG 20181119 PERF Correction post RC (Step 2)
        public static void InsertImActorPos(string pCs, DateTime pDtBusiness, int pIdA_CssCustodian)
        {
            // 20210211 [XXXXX] Call truncate car plusieurs appels posibles via TryMultiple 
            TruncateImActorPos(pCs);

            // 20210211 [XXXXX] Spheres® utilise désormais 5 requêtes pour l'alimentation

            string queryInsert;
            CommandType queryType;
            Dictionary<string, object> dbParameterValues;

            for (int i = 1; i < 6; i++)
            {
                DataContractResultSets dataContractResultSetsEnumVal = (DataContractResultSets)Enum.Parse(typeof(DataContractResultSets), $"INSERTIMACTORPOS{i}");
                queryInsert = DataContractHelper.GetQuery(dataContractResultSetsEnumVal);
                queryType = DataContractHelper.GetType(dataContractResultSetsEnumVal);
                if (i < 5)
                {
                    dbParameterValues = new Dictionary<string, object>{
                            { "DTBUSINESS", pDtBusiness },
                            { "IDA_CSSCUSTODIAN", pIdA_CssCustodian }
                    };
                }
                else
                {
                    dbParameterValues = new Dictionary<string, object>{
                            { "DTBUSINESS", pDtBusiness }
                    };
                }

#if DEBUG
                // Afin de pouvoir visualiser la requête usage de QueryParameters
                DataParameters dataParameters = new DataParameters(DataContractHelper.GetParameter(dataContractResultSetsEnumVal));
                dataParameters["DTBUSINESS"].Value = pDtBusiness;
                if (dataParameters.Contains("IDA_CSSCUSTODIAN"))
                    dataParameters["IDA_CSSCUSTODIAN"].Value = pIdA_CssCustodian;
                QueryParameters queryParameters = new QueryParameters(pCs, queryInsert, dataParameters);
#endif 

                DataHelper.ExecuteNonQuery(pCs, null, queryType, queryInsert, DataContractHelper.GetDbDataParameters(dataContractResultSetsEnumVal, dbParameterValues));
            }


            // FI 20210118 [XXXXX] L'uage d'une transaction n'est pas nécessaire
            //IDbTransaction dbTransaction = null;
            //try
            //{
            //    string cs = CSTools.SetMaxTimeOut(pCs, 480);
            //    dbTransaction = DataHelper.BeginTran(cs);
            //    DataHelper.ExecuteNonQuery(cs, dbTransaction, queryType, queryInsert, DataContractHelper.GetDbDataParameters(DataContractResultSets.INSERTIMACTORPOS, dbParameterValues));
            //    DataHelper.CommitTran(dbTransaction);

            //    if (pSvrInfoConnection.isOracle)
            //        DataHelper.UpdateStatTable(cs, String.Format("IMACTORPOS_{0}_W", pTableId).ToUpper());
            //}
            //catch (Exception)
            //{
            //    if (null != dbTransaction)
            //        DataHelper.RollbackTran(dbTransaction);
            //    throw;
            //}
            //finally
            //{
            //    if (null != dbTransaction)
            //        dbTransaction.Dispose();
            //}

        }

        // EG 20180803 PERF New avec IMACTORPOS_{BuildTableId}_W 
        public static void TruncateImActorPos(string pCs)
        {
            string queryTruncate = DataContractHelper.GetQuery(DataContractResultSets.TRUNCATEIMACTORPOS);
            if (String.IsNullOrEmpty(queryTruncate))
            {
                return;
            }
            CommandType queryType = DataContractHelper.GetType(DataContractResultSets.TRUNCATEIMACTORPOS);
            DataHelper.ExecuteNonQuery(pCs, queryType, queryTruncate);
        }

        /// <summary>
        /// Set the log method for the hierarchy building process
        /// </summary>
        /// <param name="pSetErrorWarning"></param>
        public void InitDelegates(SetErrorWarning pSetErrorWarning)
        {
            this.m_SetErrorWarning = pSetErrorWarning;
        }

        /// <summary>
        /// Set the factory node method of the hierarchy
        /// </summary>
        /// <param name="pDelegate"></param>
        public void InitNodeFactoryDelegate(GetNodeInstance<ActorNode, ActorRelationship> pDelegate)
        {
            this.m_NodeFactory = pDelegate;
        }

        /// <summary>
        /// Set the factory atribute method fo the hierarchy
        /// </summary>
        /// <param name="pDelegate"></param>
        public void InitAttributeFactoryDelegate(GetAttributeInstance<ActorRelationship> pDelegate)
        {
            this.m_AttributeFactory = pDelegate;
        }

        #endregion

        #region ICloneable Membres

        /// <summary>
        /// Build a new copy of the current hierarchy
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return SerializationHelper.Clone<ActorRoleHierarchy>(this);
        }

        #endregion
    }

    /// <summary>
    /// Actor node class (ActorRoleHierarchy element)
    /// </summary>
    [XmlRoot(ElementName = "Actor")]
    [XmlInclude(typeof(ActorNodeWithSpecificRoles))]
    public class ActorNode : ISpheresNode<ActorNode, ActorRelationship>
    {
        string m_Actor;

        int m_ActorId;

        string m_ActorName;

        bool m_Built;
        
        protected SetErrorWarning m_SetErrorWarning;

        /// <summary>
        /// Current delegate nodze factory method, invoke him to get new one node instance
        /// </summary>
        GetNodeInstance<ActorNode, ActorRelationship> m_NodeFactory;

        /// <summary>
        /// Current delegate attribute factory method, invoke him to get one new attribute instance
        /// </summary>
        protected GetAttributeInstance<ActorRelationship> m_AttributeFactory;

        List<ActorRelationship> m_RolesList = new List<ActorRelationship>();

        /// <summary>
        /// Get the roles of the actor selected from the database
        /// </summary>
        [XmlArray(ElementName = "RoleRelations")]
        [ReadOnly(true)]
        [Browsable(false)]
        public List<ActorRelationship> RolesList
        {
            get { return m_RolesList; }
            set { m_RolesList = value; }
        }

        List<ActorRelationship> m_ChildsRolesList = new List<ActorRelationship>();

        /// <summary>
        /// Get the actor childs selected from the database
        /// </summary>
        [XmlArray(ElementName = "ChildRelations")]
        [ReadOnly(true)]
        [Browsable(false)]
        public List<ActorRelationship> ChildsRolesList
        {
            get { return m_ChildsRolesList; }
            set { m_ChildsRolesList = value; }
        }

        List<ActorNode> m_ActorNodes = new List<ActorNode>();

        //RD 20170420 [23092] Add member
        List<RoleAttribute> m_Attributes = new List<RoleAttribute>();

        /// <summary>
        /// the attibutes set descending from the actor
        /// </summary>
        /// <remarks>
        /// it exists also one attribute for each role of the actor
        /// </remarks>
        [XmlArray(ElementName = "Attributes")]
        public List<RoleAttribute> Attributes
        {
            get { return m_Attributes; }
            set { m_Attributes = value; }
        }

        /// <summary>
        /// Return an empty node
        /// </summary>
        /// <remarks>
        /// you may not use this constructor to build a node, because the identifiers are empty.
        /// </remarks>
        public ActorNode() { }

        /// <summary>
        /// Return an empty well identified node 
        /// </summary>
        /// <param name="pActor">actor identifier</param>
        /// <param name="pActorId">actor internal id</param>
        /// <param name="pActorName">actor display name</param>
        public ActorNode(string pActor, int pActorId, string pActorName)
        {
            this.Identifier = pActor;

            this.Id = pActorId;

            this.DisplayName = pActorName;
        }

        #region ISpheresNode<ActorNode> Membres

        /// <summary>
        /// Get the state of the node
        /// </summary>
        [XmlAttribute(AttributeName = "built")]
        public bool Built
        {
            get { return m_Built; }
            set { m_Built = value; }
        }

        /// <summary>
        /// Actor identifier
        /// </summary>
        [XmlAttribute(AttributeName = "actor")]
        public string Identifier
        {
            get { return m_Actor; }
            set { m_Actor = value; }
        }

        /// <summary>
        /// Actor internal id
        /// </summary>
        [XmlAttribute(AttributeName = "actorid")]
        public int Id
        {
            get { return m_ActorId; }
            set { m_ActorId = value; }
        }

        /// <summary>
        /// Actor display name
        /// </summary>
        [XmlElement(ElementName = "ActorName")]
        public string DisplayName
        {
            get { return m_ActorName; }
            set { m_ActorName = value; }
        }

        /// <summary>
        /// Actor child nodes
        /// </summary>
        [XmlArray(ElementName = "ChildNodes")]
        [ReadOnly(true)]
        [Browsable(false)]
        public List<ActorNode> ChildNodes
        {
            get { return m_ActorNodes; }
            set { m_ActorNodes = value; }
        }

        /// <summary>
        /// Get the child reference having the given identifier.
        /// </summary>
        /// <param name="pIdentifier">the identifier</param>
        /// <returns>the child node having the given identifier, else null</returns>
        public ISpheresNode<ActorNode, ActorRelationship> GetChild(string pIdentifier)
        {
            ISpheresNode<ActorNode, ActorRelationship> childWithIdentifier = null;

            foreach (ISpheresNode<ActorNode, ActorRelationship> node in this.ChildNodes)
            {
                if (node.Identifier == pIdentifier)
                {
                    childWithIdentifier = node;
                    break;
                }
            }

            return childWithIdentifier;
        }

        /// <summary>
        /// Build recursively all the actor hierarchy tree
        /// </summary>
        /// <param name="pConnection">an open db connection to the database</param>
        /// <param name="pParents">list of the ancestors of the current node</param>
        /// <param name="pSessionId"></param>
        /// <returns>true if the building process get the point</returns>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        // EG 20190114 Add detail to ProcessLog Refactoring
        public virtual bool BuildNode(List<ISpheresNode<ActorNode, ActorRelationship>> pParents, IDbConnection pConnection)
        {

            this.Built = false;

            pParents.Add(this);

            ResetNode();

            Dictionary<string, object> parameterValues = new Dictionary<string, object>
            {
                { "IDA", this.Id }
            };

            // Loading relationships of the current actor...

            List<ActorRelationship> actorRelationships = DataHelper<ActorRelationship>.ExecuteDataSet(
                pConnection,
                DataContractHelper.GetType(DataContractResultSets.ACTORRELATIONSHIP),
                DataContractHelper.GetQuery(DataContractResultSets.ACTORRELATIONSHIP),
                DataContractHelper.GetDbDataParameters(DataContractResultSets.ACTORRELATIONSHIP, parameterValues));

            // Building the role and child lists...

            if (actorRelationships != null)
            {
                foreach (ActorRelationship relation in actorRelationships)
                {
                    if (relation.ActorId == relation.RoleOwnerId)
                    {
                        // case 1 : the actor is the owner of the role underlying to the current relation

                        RolesList.Add(relation);
                    }
                    else
                    {
                        // case 2 : the current relation is with regards to the actor, an actor child is the owner of the underlying role 

                        ChildsRolesList.Add(relation);
                    }
                }

                // Building the child-nodes list (based on the current ChildsList), and calling recursively the BuildNode for any added node

                foreach (ActorRelationship relation in ChildsRolesList)
                {
                    bool childDoesNotExist = this.GetChild(relation.RoleOwner) == null;

                    bool isNotCrossReference = !this.IsCrossReference(pParents, relation);

                    // Adding the child just when the child does not exist
                    if (childDoesNotExist)
                    {
                        if (isNotCrossReference)
                        {
                            ActorNode newNode = (ActorNode)m_NodeFactory.Invoke(pConnection, relation);

                            newNode.InitDelegate(m_SetErrorWarning);

                            newNode.InitNodeFactoryDelegate(this.m_NodeFactory);

                            newNode.InitAttributeFactoryDelegate(this.m_AttributeFactory);

                            this.Built = newNode.BuildNode(pParents, pConnection);

                            if (this.Built)
                            {
                                ChildNodes.Add(newNode);
                            }
                            else
                            {
                                m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1015), 0, new LogParam(relation.RoleOwner), new LogParam(relation.RoleOwnerName)));
                            }
                        }
                        else
                        // cross-reference found!
                        {

                            m_SetErrorWarning.Invoke(ProcessStateTools.StatusErrorEnum);

                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1022), 0,
                                new LogParam(Identifier),
                                new LogParam(Convert.ToString(Id)),
                                new LogParam(relation.RoleOwner),
                                new LogParam(Convert.ToString(relation.RoleOwnerId))));
                        }
                    } // childDoesNotExist
                }

            } // end if relationships != null

            this.Built = true;

            pParents.Remove(this);

            // RD 20170420 [23092] Add
            BuildNodeAttribute(pParents, pConnection);

            return this.Built;
        }

        /// <summary>
        /// Load the attribute node
        /// </summary>
        /// <param name="pParents">list of the ancestors of the current node</param>
        /// <param name="pConnection">an open connection</param>        
        //RD 20170420 [23092] Add method
        // EG 20190114 ProcessLog delegate Refactoring
        public virtual void BuildNodeAttribute(List<ISpheresNode<ActorNode, ActorRelationship>> pParents, IDbConnection pConnection)
        {
            RoleAttribute attribute = new RoleAttribute(this.Id, 0);
            
            this.Built = attribute.BuildAttribute(pConnection);
            this.Attributes.Add(attribute);
        }

        /// <summary>
        /// Get recursively all the nodes of the actor hierarchy matching the given predicate
        /// </summary>
        /// <param name="searchingPred">lambda expression identifying the node property you are interested in</param>
        /// <returns>all the nodes of the actor hierarchy that match the given predicate</returns>
        /// <remarks>the predicated is applied as recursive condition, then the recursion stops when a node
        /// does not match the given predicate</remarks>
        public List<ActorNode> FindChilds(Predicate<ActorNode> searchingPred)
        {
            List<ActorNode> actors = new List<ActorNode>();

            List<ActorNode> matchingChilds = this.ChildNodes.FindAll(searchingPred);

            actors.AddRange(matchingChilds);

            foreach (ActorNode child in matchingChilds)
            {
                actors.AddRange(child.FindChilds(searchingPred));
            }

            // 20110427 MF - grouping duplicate nodes

            actors =
                actors
                .GroupBy(actornode => actornode.Id)
                .Select(actorgroup => actorgroup.First()).ToList();

            return actors;
        }

        /// <summary>
        /// Find all the ancestors of the current node that match the predicate
        /// </summary>
        /// <param name="pSearchRoot">actor node limit of the parent research</param>
        /// <param name="searchingPred">the predicate the ancestors have to match</param>
        /// <returns>all the parents of the actor hierarchy that match the given predicate</returns>
        /// <remarks>the predicate is used as recursive condition, then the recursion stops when a node
        /// does not match the given predicate</remarks>
        public List<ActorNode> FindAncestors(ActorNode pSearchRoot, Predicate<ActorNode> searchingPred)
        {

            // 1. get all the nodes from the search root

            List<ActorNode> nodes =
                (from node
                    in pSearchRoot.FindChilds(searchingPred).
                    Union(new ActorNode[] { pSearchRoot }.ToList().FindAll(searchingPred))
                 select node).ToList();

            // 2. find the direct parents of the current node 

            IEnumerable<ActorRelationship> rolesGroupedByWithRegardToActorId =
                this.RolesList
                .GroupBy(role => role.RoleWithRegardToActorId)
                .Select(roleGroup => roleGroup.First());

            IEnumerable<ActorNode> nodesGroupedById =
                nodes
                .GroupBy(node => node.Id)
                .Select(group => group.First());

            List<ActorNode> parents =
                (from role
                    in rolesGroupedByWithRegardToActorId
                 from node
                     in nodesGroupedById
                 where node.Id == role.RoleWithRegardToActorId && node.Id != this.Id
                 select node).
                ToList();

            // 2. find all the descendants of the current node 

            List<ActorNode> childs =
                (from node
                    in this.FindChilds(searchingPred)
                 select node).
                 ToList();

            // 3. check the existence of cross-references between the descendants and the direct parents, remove the parent in case of 

            List<ActorNode> crossReferences =
                (from child
                     in childs
                 from parent
                    in parents
                 where child.Id == parent.Id
                 select parent).
                 ToList();

            List<ActorNode> filteredParents = parents.Except(crossReferences).ToList();

            // 3. recursive call to find all the ancestors of the node

            List<ActorNode> ancestors =
                (from parent
                    in filteredParents
                 from ancestor
                    in parent.FindAncestors(pSearchRoot, searchingPred)
                 select ancestor).
                ToList();

            foreach (ActorNode ancestor in ancestors)
            {
                if (!filteredParents.Contains(ancestor))
                {
                    filteredParents.Add(ancestor);
                }
            }

            return filteredParents;
        }

        /// <summary>
        /// Set the logging method for the node creation process.
        /// </summary>
        /// <param name="pSetErrorWarning"></param>
        /// FI 20200623 [XXXXX] Add SetErrorWarning
        public void InitDelegate(SetErrorWarning pSetErrorWarning)
        {
            this.m_SetErrorWarning = pSetErrorWarning;
        }

        /// <summary>
        /// Set the node factory method used to return specific node-type childs . 
        /// </summary>
        /// <param name="pDelegate">the factory method</param>
        public void InitNodeFactoryDelegate(GetNodeInstance<ActorNode, ActorRelationship> pDelegate)
        {
            this.m_NodeFactory = pDelegate;
        }

        /// <summary>
        /// Set the attribute factory method used to get specific node-type attributes 
        /// </summary>
        /// <param name="pDelegate"></param>
        public void InitAttributeFactoryDelegate(GetAttributeInstance<ActorRelationship> pDelegate)
        {
            this.m_AttributeFactory = pDelegate;
        }

        #endregion

        #region ICloneable Membres

        /// <summary>
        /// Build a new copy of the current node
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return SerializationHelper.Clone<ActorNode>(this);
        }

        #endregion

        /// <summary>
        /// Find the first level parents of he current node
        /// </summary>
        /// <param name="pSearchRoot">upper limit of the parents research</param>
        /// <param name="searchingPred">the predicate the ancestors have to match</param>
        /// <returns>the first level parents of the actor that match the given predicate</returns>
        /// <remarks>the predicate FOLLOWS the recursive condition, then the recursion does NOT stop when a node
        /// does not match the given predicate</remarks>
        public List<ActorNode> FindParents(ActorNode pSearchRoot, Predicate<ActorNode> searchingPred)
        {
            List<ActorNode> parents = (
                from ancestor
                in this.FindAncestors(pSearchRoot, node => true)
                from role in this.RolesList
                where role.RoleWithRegardToActorId == ancestor.Id
                select ancestor
                ).ToList();

            List<ActorNode> matchingParents = parents.FindAll(searchingPred);

            List<ActorNode> noMatchingParents = parents.Except(matchingParents).ToList();

            // for each parent that does not match the given predicate we continue the parents research to find an ancestor satisfying the 
            //  given predicate conditions.

            matchingParents.AddRange((
                from noMatchingParent
                    in noMatchingParents
                from matchingHigherParent in noMatchingParent.FindParents(pSearchRoot, searchingPred)
                select matchingHigherParent));

            // filter duplicates

            matchingParents = matchingParents
                .GroupBy(parent => parent.Id, parent => parent)
                .Select(group => group.First())
                .ToList();

            return matchingParents;
        }

        /// <summary>
        /// Reste the node lists
        /// </summary>
        public virtual void ResetNode()
        {
            ChildNodes.Clear();

            ChildsRolesList.Clear();

            RolesList.Clear();

            //RD 20170420 [23092] Add 
            Attributes.Clear();
        }

        private bool IsCrossReference(List<ISpheresNode<ActorNode, ActorRelationship>> pParents, ActorRelationship relation)
        {
            List<ISpheresNode<ActorNode, ActorRelationship>> crossreferences =
                (from parent in pParents
                 where IsCrossReference(parent, relation)
                 select parent).ToList();

            return crossreferences.Count > 0;
        }

        private bool IsCrossReference(ISpheresNode<ActorNode, ActorRelationship> pParent, ActorRelationship relation)
        {
            return pParent.Id == relation.RoleOwnerId;
        }

    }

    /// <summary>
    /// Specialized node class for Spheres actors with specific roles
    /// </summary>
    /// <example>we create special nodes for actor nodes that have roles MARGINREQOFFICE, CSS or CLEARINGCOMPART</example>
    [XmlRoot(ElementName = "ActorWithSpecificRoles")]
    public class ActorNodeWithSpecificRoles : ActorNode
    {

        /// <summary>
        /// Flag indicating when the current attribute has been defined for a MARGINREQOFFICE actor 
        /// descending either from a clearer either from the entity. true when descending from a clearer.
        /// </summary>
        [XmlAttribute(AttributeName = "fromclearer")]
        public bool IsDescendingFromClearer
        {
            get;
            set;
        }

        List<RoleAttribute> m_RoleSpecificAttributes = new List<RoleAttribute>();

        /// <summary>
        /// the attibutes set descending from the roles of the actor
        /// </summary>
        /// <remarks>
        /// for each role of the actor one new attribute is added
        /// </remarks>
        [XmlArray(ElementName = "RoleSpecificAttributes")]
        public List<RoleAttribute> RoleSpecificAttributes
        {
            get { return m_RoleSpecificAttributes; }
            set { m_RoleSpecificAttributes = value; }
        }

        [XmlAttribute(AttributeName = "rootid")]
        public int RootId { get; set; }

        /// <summary>
        /// Current delegate node check method, invoke him to verify the current attribute/node properties
        /// </summary>
        protected GetNodeProperty<ISpheresNode<ActorNode, ActorRelationship>> m_NodeVerification;

        /// <summary>
        /// get an empty unidentified node
        /// </summary>
        /// <remarks>
        /// the building process can not be done on unidentified nodes
        /// </remarks>
        public ActorNodeWithSpecificRoles()
            : base()
        { }

        /// <summary>
        /// Get an empty well identified node
        /// </summary>
        /// <param name="pActor">identifier of the actor</param>
        /// <param name="pActorId">internal id of the actor</param>
        /// <param name="pActorName">display name of the actor</param>
        public ActorNodeWithSpecificRoles(string pActor, int pActorId, string pActorName)
            : base(pActor, pActorId, pActorName)
        {

        }

       
        /// <summary>
        /// Load the attribute node
        /// </summary>
        /// <param name="pParents">list of the ancestors of the current node</param>
        /// <param name="pConnection">an open connection</param>
        //RD 20170420 [23092] Add method
        // EG 20190114 ProcessLog delegate and Add detail to ProcessLog Refactoring
        public override void BuildNodeAttribute(List<ISpheresNode<ActorNode, ActorRelationship>> pParents, IDbConnection pConnection)
        {
            foreach (ActorRelationship role in base.RolesList)
            {
                ISpheresNodeAttribute attribute = m_AttributeFactory.Invoke(pConnection, role);

                if (attribute != null)
                {
                    

                    
                    //string attributeVerificationError = null;
                    SysMsgCode attributeVerificationError = default;
                    bool isDescendingByClearer = false;
                    int rootId = 0;

                    base.Built =
                        attribute.BuildAttribute(pConnection) &&
                        m_NodeVerification.Invoke(
                            pParents, this, out isDescendingByClearer, out rootId, out attributeVerificationError);

                    if (base.Built)
                    {
                        this.IsDescendingFromClearer = isDescendingByClearer;
                        this.RootId = rootId;
                    }
                    else if (attributeVerificationError != null)
                    {
                        string parentList = Cst.None;
                        string roleList = Cst.None;

                        if (pParents != null && pParents.Count > 0)
                        {
                            parentList = (from parent in pParents select parent.Identifier).Aggregate((acc, next) => String.Format("{0};{1}", acc, next));
                        }

                        // PM 20140115 [19489] Ajout de la liste de rôle de l'acteur dans le message d'erreur
                        if ((RolesList != default(List<ActorRelationship>)) && (RolesList.Count > 0))
                        {
                            roleList = (from roleInfo in RolesList select roleInfo.RoleName).Aggregate((acc, next) => String.Format("{0};{1}", acc, next));
                        }
                        // PM 20131025 [19100]
                        Logger.Log(new LoggerData(LogLevelEnum.Error, attributeVerificationError, 0, new LogParam(Identifier), new LogParam(parentList), new LogParam(roleList)));
                    }

                    this.RoleSpecificAttributes.Add((RoleAttribute)attribute);
                }
            }
        }

        /// <summary>
        /// Get a new actor instance
        /// </summary>
        /// <returns>a new ActorNodeWithSpecificRoles instance </returns>
        public override object Clone()
        {
            return SerializationHelper.Clone<ActorNodeWithSpecificRoles>(this);
        }

        /// <summary>
        /// reset the actor collections
        /// </summary>
        public override void ResetNode()
        {
            base.ResetNode();

            RoleSpecificAttributes.Clear();
        }

        /// <summary>
        /// Get all the attributes/roles of the given type
        /// </summary>
        /// <typeparam name="T">role type</typeparam>
        /// <returns>the attributes/roles list</returns>
        public T[] GetRolesTypeOf<T>() where T : RoleAttribute
        {
            T[] attributes = (from attribute in this.RoleSpecificAttributes
                              where attribute is T
                              select (T)attribute
            ).ToArray();
            return attributes;
        }

        /// <summary>
        /// Set the node verification method used to verify specific node properties . 
        /// </summary>
        /// <param name="pDelegate">the verification method</param>
        public void InitNodeVerificationDelegate(GetNodeProperty<ISpheresNode<ActorNode, ActorRelationship>> pDelegate)
        {
            this.m_NodeVerification = pDelegate;
        }

    }

    /// <summary>
    /// Class representing a generic role attribute
    /// </summary>
    [XmlRoot(ElementName = "RoleSpecificParameters")]
    public class RoleAttribute : ISpheresNodeAttribute
    {
        private int m_IdNode;

        private int m_IdParentNode;

        private bool m_Built;

        

        #region ISpheresNodeAttribute Membres

        /// <summary>
        /// The id of the node owning the attribute
        /// </summary>
        [XmlAttribute(AttributeName = "node")]
        public int IdNode
        {
            get { return m_IdNode; }

            set { m_IdNode = value; }
        }

        /// <summary>
        /// Actor internal id, it is the id of the actor parent of the actor/role relation. Can be 0.
        /// </summary>
        [XmlAttribute(AttributeName = "rolemarginwithregardsto")]
        public int IdParentNode
        {
            get { return m_IdParentNode; }
            set { m_IdParentNode = value; }
        }

        /// <summary>
        /// Attribute status
        /// </summary>
        [XmlAttribute(AttributeName = "built")]
        public bool Built
        {
            get { return m_Built; }

            set { m_Built = value; }
        }

        //RD 20170420 [23092] Ajouter la liste des Books de chaque acteur
        private List<Books> m_BooksOfActor = new List<Books>();

        /// <summary>
        /// Books list of the Node actor
        /// </summary>
        [XmlArray(ElementName = "BooksOfActor")]
        [XmlArrayItem(ElementName = "Book")]
        [ReadOnly(true)]
        [Browsable(false)]
        internal List<Books> BooksOfActor
        {
            get { return m_BooksOfActor; }
            set { m_BooksOfActor = value; }
        }

        //RD 20170420 [] Déplacer ici ce membre qui étatit dans la classe "RoleMarginReqOfficeAttribute"
        private List<BookNode> m_Books = new List<BookNode>();

        /// <summary>
        /// Book nodes list
        /// </summary>
        [XmlArray(ElementName = "Books")]
        [XmlArrayItem(ElementName = "Book")]
        [ReadOnly(true)]
        [Browsable(false)]
        public List<BookNode> Books
        {
            get { return m_Books; }
            set { m_Books = value; }
        }

        /// <summary>
        /// Empty identified attribute
        /// </summary>
        // PM 20200129 Constructeur necessaire à la serialisation en mode log Full
        public RoleAttribute() { }

        /// <summary>
        /// Empty identified attribute
        /// </summary>
        /// <param name="pIdNode">internal id of the actor owning the attribute</param>
        /// <param name="pIdParentNode">optional father node relative to the relationship base of the current attribute</param>
        public RoleAttribute(int pIdNode, int pIdParentNode)
        {
            this.IdNode = pIdNode;

            this.IdParentNode = pIdParentNode;
        }


        /// <summary>
        /// Load the attribute datas
        /// </summary>
        /// <param name="pConnection">open connection</param>
        /// <returns></returns>
        public virtual bool BuildAttribute(IDbConnection pConnection)
        {
            ResetAttribute();

            //RD 20170420 [23092] Charger la liste des Books de chaque acteur
            this.Built = false;

            Dictionary<string, object> parameterValues = new Dictionary<string, object>
            {
                { "IDA", this.IdNode }
            };

            BooksOfActor = DataHelper<Books>.ExecuteDataSet(
                pConnection,
                DataContractHelper.GetType(DataContractResultSets.BOOKS),
                DataContractHelper.GetQuery(DataContractResultSets.BOOKS),
                DataContractHelper.GetDbDataParameters(DataContractResultSets.BOOKS, parameterValues));

            if (BooksOfActor != null)
            {
                BuildBooks();
            }

            return this.Built = true;
        }

        /// <summary>
        /// Rest attribute collections
        /// </summary>
        public virtual void ResetAttribute()
        {
            //RD 20170420 [23092] Add
            this.BooksOfActor.Clear();
            //RD 20170420 [23092] Déplacement à partir de la méthode "RoleMarginReqOfficeAttribute.ResetAttribute()"
            this.Books.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void BuildBooks()
        {
            foreach (Books book in BooksOfActor)
            {
                BookNode node = new BookNode(book.Book, book.BookId, book.BookName, false);

                this.Books.Add(node);
            }
        }

        
        #endregion

        #region ICloneable members

        /// <summary>
        /// Build a new copy of the current hierarchy instance
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return SerializationHelper.Clone<RoleAttribute>(this);
        }

        #endregion
    }

    /// <summary>
    /// Class representing a book 
    /// </summary>
    [XmlRoot(ElementName = "Book")]
    public class BookNode : ISpheresNodeLeaf
    {
        string m_Book;

        int m_BookId;

        string m_BookName;

        bool m_IsIMRBook;

        /// <summary>
        /// Flag indicating this book as IMR
        /// </summary>
        public bool IsIMRBook
        {
            get { return m_IsIMRBook; }
            set { m_IsIMRBook = value; }
        }

        /// <summary>
        /// Get an empty book node
        /// </summary>
        public BookNode()
        { }

        /// <summary>
        /// Get a book node
        /// </summary>
        /// <param name="pBook">Book identifier</param>
        /// <param name="pBookId">Book internal id</param>
        /// <param name="pBookName">Book dislpay name</param>
        /// <param name="pIsIMRBook">IMR flag</param>
        public BookNode(string pBook, int pBookId, string pBookName, bool pIsIMRBook)
        {
            this.Identifier = pBook;

            this.Id = pBookId;

            this.DisplayName = pBookName;

            this.IsIMRBook = pIsIMRBook;
        }

        #region ISpheresNodeLeaf Membres

        /// <summary>
        /// Book identifier
        /// </summary>
        [XmlAttribute(AttributeName = "book")]
        public string Identifier
        {
            get { return m_Book; }
            set { m_Book = value; }
        }

        /// <summary>
        /// Book internal id
        /// </summary>
        [XmlAttribute(AttributeName = "bookid")]
        public int Id
        {
            get { return m_BookId; }
            set { m_BookId = value; }
        }

        /// <summary>
        /// Book display name
        /// </summary>
        [XmlElement(ElementName = "BookName")]
        public string DisplayName
        {
            get { return m_BookName; }
            set { m_BookName = value; }
        }

        #endregion
    }
}
