using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Reflection;


namespace Scv_Entities
{
    public static class Extensions
    {
        public static void CopyPropertiesClassToClassSpecifiedProperty<T, TT>(this T objGet, TT objSet)
            where T : EntityObject
        {
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var propertiesTest = typeof(TT).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                var propertyTt = propertiesTest.SingleOrDefault(t => t.Name == property.Name);

                if (!CopyPropertiesCheckProperty(property))
                    continue;

                if (propertyTt == null) continue;
                var value = property.GetValue(objGet, null);
                propertyTt.SetValue(objSet, value, null);
            }
        }

        private static bool CopyPropertiesCheckProperty(PropertyInfo property)
        {
            if (property == null)
                return true;

            var propertyType = ((property)).PropertyType;

            if (propertyType == typeof(string))
                return true;

            if (propertyType == typeof(int) || propertyType == typeof(int?))
                return true;

            if (propertyType == typeof(long) || propertyType == typeof(long?))
                return true;

            if (propertyType == typeof(float) || propertyType == typeof(float?))
                return true;

            if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                return true;

            if (propertyType == typeof(sbyte) || propertyType == typeof(sbyte?))
                return true;

            if (propertyType == typeof(uint) || propertyType == typeof(uint?))
                return true;

            if (propertyType == typeof(short) || propertyType == typeof(short?))
                return true;

            if (propertyType == typeof(ushort) || propertyType == typeof(ushort?))
                return true;

            if (propertyType == typeof(long) || propertyType == typeof(long?))
                return true;

            if (propertyType == typeof(ulong) || propertyType == typeof(ulong?))
                return true;

            if (propertyType == typeof(double) || propertyType == typeof(double?))
                return true;

            if (propertyType == typeof(char) || propertyType == typeof(char?))
                return true;

            if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                return true;

            if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                return true;

            return false;
        }

        public static EntityObject AttachUpdated(this IN_VIAEntities context, EntityObject objectDetached)
        {
            //DA RIPRISTINARE IN CASO DI CHIAMATA DA WCF SERVICE
            //if (objectDetached.EntityState == EntityState.Detached)
            //{

            object currentEntityInDb = null;
            EntityObject resultObj = null;

            if (context.TryGetObjectByKey(objectDetached.EntityKey, out currentEntityInDb))
            {

                resultObj = context.ApplyCurrentValues(objectDetached.EntityKey.EntitySetName, objectDetached);

                //(CDLTLL)Apply property changes to all referenced entities in context 
                //context.ApplyReferencePropertyChanges((IEntityWithRelationships)objectDetached,
                //                                      (IEntityWithRelationships)currentEntityInDb); //Custom extensor 


            }

            else
            {

                throw new ObjectNotFoundException();

            }

            //}

            return resultObj;
        }

        public static void ApplyReferencePropertyChanges(this IN_VIAEntities context,
                                                         IEntityWithRelationships newEntity,
                                                         IEntityWithRelationships oldEntity)
        {
            foreach (var relatedEnd in oldEntity.RelationshipManager.GetAllRelatedEnds())
            {
                var oldRef = relatedEnd as EntityReference;
                if (oldRef != null)
                {
                    var newRef = newEntity.RelationshipManager.GetRelatedEnd(oldRef.RelationshipName, oldRef.TargetRoleName) as EntityReference;
                    oldRef.EntityKey = newRef.EntityKey;
                }
            }
        }

        public static bool CheckIfObjectUpdated(this IN_VIAEntities context,
                                                 EntityObject updatedValuesObject,
                                                 EntityObject oldValuesObject)
        {
            bool modified = false;
            string tableName = "";

            try
            {
                tableName = updatedValuesObject.EntityKey.EntitySetName;
            }
            catch
            {
                try
                {
                    tableName = oldValuesObject.EntityKey.EntitySetName;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            foreach (PropertyInfo infoUpdated in updatedValuesObject.GetType().GetProperties())
            {



                    foreach (PropertyInfo infoOld in oldValuesObject.GetType().GetProperties())
                    {
                        if (infoUpdated.Name != null && infoOld.Name != null)
                        {
                            if (infoUpdated.Name == infoOld.Name
                                && infoUpdated.PropertyType.BaseType.Name != "EntityObject"
                                && infoUpdated.PropertyType.BaseType.Name != "EntityReference"
                                 && infoUpdated.PropertyType.BaseType.Name != "RelatedEnd"
                                && !(infoUpdated.Name == "Id_User" 
                                     || infoUpdated.Name == "Dt_Update" 
                                     || infoUpdated.Name == "Item" 
                                     || infoUpdated.Name == "EntityState"
                                     || infoUpdated.Name == "IsEmpty"
                                     || infoUpdated.Name == "Editing"
                                     || infoUpdated.Name == "IsErasable"
                                     )
                               )
                            {
                                string nm = infoUpdated.Name;
                                try
                                {
                                    object updatedValue = infoUpdated.GetValue(updatedValuesObject, null);
                                    object oldValue = infoOld.GetValue(oldValuesObject, null);

                                    if ((updatedValue != null && oldValue != null))
                                    {
                                        if (!updatedValue.Equals(oldValue))
                                        {
                                            modified = true;
                                        }
                                    }
                                    else if ((updatedValue == null && oldValue != null) || (updatedValue != null && oldValue == null))
                                    {
                                        modified = true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }


                            }
                        }



                    }


            }

            return modified;
        }




    }
}
