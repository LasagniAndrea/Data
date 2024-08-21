using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//
using EFS.ApplicationBlocks.Data;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.Spheres.Hierarchies;

namespace EFS.SpheresRiskPerformance.Hierarchies
{
    /// <summary>
    /// Helper class containing the tools to instanciate extended ISpheresNode/ISpheresNodeAttribute types objects,
    /// defined for the risk evaluation process.
    /// 
    /// </summary>
    /// <remarks>Including custom Risk Performance implementation of the factory methods, 
    /// as well as the specific roles identifiers and types</remarks>
    static public class RiskHierarchyFactory
    {
        

        /// <summary>
        /// the extra roles types are used to serialize structures containing RoleAttribute extended types objects
        /// </summary>
        public static Type[] ExtraRolesTypes =
            new Type[] { typeof(RoleMarginReqOfficeAttribute)
                /*, typeof(RoleEntityAttribute), 
                 * typeof(RoleClearerCSSAttribute)*/ 
            };

        /// <summary>
        /// Factory methode creating ISpheresNodeAttribute extended types instances
        /// </summary>
        /// <param name="pConnection">an open connection</param>
        /// <param name="pChildRelation">the actor/roole relation base base of the attribute</param>
        /// <returns>true when the creation process ends well</returns>
        public static ISpheresNodeAttribute GetAttributeInstance(IDbConnection pConnection, ActorRelationship pChildRelation)
        {

            ISpheresNodeAttribute attribute = null;

            switch (pChildRelation.Role)
            {
                case RolesCollection.ROLEMARGINREQOFFICE:

                    attribute = new RoleMarginReqOfficeAttribute
                        (pChildRelation.RoleOwnerId, pChildRelation.RoleWithRegardToActorId, 
                        pChildRelation.ElementEnabledFrom, pChildRelation.ElementDisabledFrom);

                    break;

                case RolesCollection.ROLECLEARER:
                case RolesCollection.ROLECSS:

                    // actors with role clearer or css share the same attribute type

                    break;

                case RolesCollection.ROLEENTITY:

                    break;

                default:

                    break;

            }

            return attribute;
        }

        /// <summary>
        /// Factory methode creating ISpheresNode extended types instances
        /// </summary>
        /// <param name="pConnection">open connection</param>
        /// <param name="pChildRelation">relationship with the CHILD node that will be returned by the factory method</param>
        /// <returns>a new node instance</returns>
        public static ISpheresNode<ActorNode, ActorRelationship> 
            GetNodeInstance(IDbConnection pConnection, ActorRelationship pChildRelation)
        {
            ISpheresNode<ActorNode, ActorRelationship> newNode = null;

            Dictionary<string, object> parameterValues = new Dictionary<string, object>
            {
                { "IDA", pChildRelation.RoleOwnerId }
            };

            // Get the roles list for the child id in order to get ALL its roles, 
            //  because the factory method is called by the current parent node which 
            //  has loaded just the child roles he is with regards to.

            List<ActorRelationship> actorRoles = DataHelper<ActorRelationship>.ExecuteDataSet(
                pConnection,
                DataContractHelper.GetType(DataContractResultSets.ACTORROLES),
                DataContractHelper.GetQuery(DataContractResultSets.ACTORROLES),
                DataContractHelper.GetDbDataParameters(DataContractResultSets.ACTORROLES, parameterValues));

            bool specific = false;

            foreach (ActorRelationship role in actorRoles)
            {
                if (role.Role == RolesCollection.ROLEMARGINREQOFFICE ||
                    role.Role == RolesCollection.ROLECSS ||
                    role.Role == RolesCollection.ROLECLEARER ||
                    role.Role == RolesCollection.ROLEENTITY)
                {
                    specific = true;

                    break;
                }
            }

            if (specific)
            {
                ActorNodeWithSpecificRoles newSpecificNode =
                    new ActorNodeWithSpecificRoles(pChildRelation.RoleOwner, pChildRelation.RoleOwnerId, pChildRelation.RoleOwnerName);

                newSpecificNode.InitNodeVerificationDelegate(RiskHierarchyFactory.SetHierarchyInformation);

                newNode = newSpecificNode;
            }
            else
                newNode = new ActorNode(pChildRelation.RoleOwner, pChildRelation.RoleOwnerId, pChildRelation.RoleOwnerName);

            return newNode;
        }

        /// <summary>
        /// Verify if the input node is descending either by a clearer or by an entity
        /// </summary>
        /// <param name="pParents">list of the parents of the current node</param>
        /// <param name="pCurrentNode">current node to be verified</param>
        /// <param name="opIsDescendingFromClearer">true when the node is descending by a clearer. Warning: when the return value is false,
        /// this output value is not trusted</param>
        /// <param name="opRootActorId">id of the actor node that is the root of the hierarchy. Warning: when the return value is false,
        /// this output value is not trusted</param>
        /// <param name="opError">giving additional information in case the current node is not descending by a good hierarchy</param>
        /// <returns>true when the checked node is descending form a good hierarchy: it descends by one clearer or one or more entities</returns>
        
        //public static bool SetHierarchyInformation(
        //    List<ISpheresNode<ActorNode, ActorRelationship>> pParents, ISpheresNode<ActorNode, ActorRelationship> pCurrentNode,
        //    out bool opIsDescendingFromClearer, out int opRootActorId, out string opError)
        public static bool SetHierarchyInformation(
            List<ISpheresNode<ActorNode, ActorRelationship>> pParents, ISpheresNode<ActorNode, ActorRelationship> pCurrentNode,
            out bool opIsDescendingFromClearer, out int opRootActorId, out SysMsgCode opError)
        {
            opError = null;
            opIsDescendingFromClearer = false;
            opRootActorId = -1;

            bool res = false;

            List<ISpheresNode<ActorNode, ActorRelationship>> parentsAndCurrentActor = new List<ISpheresNode<ActorNode, ActorRelationship>>();

            parentsAndCurrentActor.AddRange(pParents);
            parentsAndCurrentActor.Add(pCurrentNode);

            // 

            IEnumerable<ISpheresNode<ActorNode, ActorRelationship>> rootEntity =
                from actorNode
                    in parentsAndCurrentActor
                where actorNode is ActorNode
                from relation
                    in ((ActorNode)actorNode).RolesList
                where
                    relation.Role == RolesCollection.ROLEENTITY
                select actorNode;

            IEnumerable<ISpheresNode<ActorNode, ActorRelationship>> rootClearer =
                from actorNode
                    in parentsAndCurrentActor
                where actorNode is ActorNode
                from relation
                    in ((ActorNode)actorNode).RolesList
                where
                    relation.Role == RolesCollection.ROLECSS
                    || relation.Role == RolesCollection.ROLECLEARER
                select actorNode;

            bool isDescendingFromEntity = rootEntity.Count() >= 1;

            bool isDescendingFromClearer = rootClearer.Count() == 1;

            res = (isDescendingFromEntity ^ isDescendingFromClearer);

            if (res)
            {
                opIsDescendingFromClearer = isDescendingFromClearer;

                if (opIsDescendingFromClearer)
                {
                    opRootActorId = rootClearer.First().Id;
                }
                else
                {
                    opRootActorId = rootEntity.First().Id;
                }
            }
            else 
            {
                opError = new SysMsgCode(SysCodeEnum.SYS, 1007);
            }
            return res;
        }
    }
}