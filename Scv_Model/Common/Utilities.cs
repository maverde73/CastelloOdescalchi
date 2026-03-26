using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Reflection;
using System.Linq.Expressions;
using System.Data.Objects.DataClasses;
using System.Collections.ObjectModel;
using System.Collections;
using System.Data;
using System.Data.Entity;

namespace Scv_Model.Common
{
    public class Utilities
    {
        #region Enum
        public enum SQLOperator
        {
            Equal,
            Like,
            StartWith,
            EndWith,
            GreaterThan,
            GreaterThanEqual,
            LessThan,
            LessThanEqual,
            In,
            Between,
            NotEqual

        }

        public enum SQLCondition
        {
            And,
            Or
        }

        public enum OrderBy
        {
            Asc,
            Desc
        }

        public enum ValueType
        {
            String,
            Int,
            Bool,
            DateTime,
            Decimal,
            Double,
            Nullable
        }

        #endregion

        #region Methods

        public object GetParameter(object value, ValueType ParameterType)
        {
            object parameter = null;
            switch (ParameterType)
            {
                case ValueType.String:
                    {
                        if ((string)value != "")
                            parameter = Convert.ToString(value);
                    }
                    break;

                case ValueType.Int:
                    {
                        //if ((Int32)value != 0)
                        parameter = Convert.ToInt32(value);
                    }
                    break;

                case ValueType.DateTime:
                    {
                        if ((DateTime)value != DateTime.MinValue)
                            parameter = Convert.ToDateTime(value);
                    }
                    break;

                case ValueType.Bool:
                    {
                        parameter = Convert.ToBoolean(value);
                    }
                    break;

                case ValueType.Double:
                    {
                        parameter = Convert.ToDouble(value);
                    }
                    break;
                //case ValueType.Nullable:
                //    {
                //        parameter = value;
                //    }
                //    break;
            }

            return parameter;

        }

        public string EnumToString(SQLOperator EnumSQL)
        {
            string SQL = "";
            switch (EnumSQL)
            {
                case SQLOperator.Equal:
                    SQL = " = ";
                    break;
                case SQLOperator.EndWith:
                    SQL = " EndWith ";
                    break;
                case SQLOperator.GreaterThan:
                    SQL = " > ";
                    break;
                case SQLOperator.GreaterThanEqual:
                    SQL = " >= ";
                    break;
                case SQLOperator.LessThan:
                    SQL = " < ";
                    break;
                case SQLOperator.LessThanEqual:
                    SQL = " <= ";
                    break;
                case SQLOperator.Like:
                    SQL = " LIKE ";
                    break;
                case SQLOperator.StartWith:
                    SQL = " StartWith ";
                    break;
                case SQLOperator.NotEqual:
                    SQL = "!=";
                    break;
            }
            return SQL;
        }

        public string EnumToString(SQLCondition Condition)
        {
            string stringCondition = "";
            switch (Condition)
            {
                case SQLCondition.Or:
                    stringCondition = "||";
                    break;
                case SQLCondition.And:
                    stringCondition = "&&";
                    break;

            }
            return stringCondition;
        }

        public string EnumToString(OrderBy Order)
        {
            string stringOrder = "";
            switch (Order)
            {
                case OrderBy.Asc:
                    stringOrder = "Asc";
                    break;
                case OrderBy.Desc:
                    stringOrder = "Desc";
                    break;

            }
            return stringOrder;
        }

        public static T getField<T>(System.Data.Objects.DataClasses.EntityObject ent,string FieldName)
        {
            Type t = ent.GetType();
            PropertyInfo pinfo = t.GetProperty(FieldName);
            return (T)pinfo.GetValue(ent, null);
        }

        public string GetParameterName(string Field)
        {
            string ParameterName = "";
            if (Field.LastIndexOf(".") != -1)
                ParameterName = Field.Substring(Field.LastIndexOf(".") + 1);
            else
                ParameterName = Field;
            return ParameterName;
        }

        public string InBetweenQuery(SQLOperator SqlOperator, string field, List<object> values, string Condition, string parameter)
        {
            string Query = "";
            if(SqlOperator == SQLOperator.In)
                Query = InQuery(field, values, Condition, parameter);
            else
                Query = BetweenQuery(field, values, Condition, parameter);
            return Query;
        }

        public string BetweenQuery(string field, List<object> values, string Condition, string parameter)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append("(");
            sb.Append("(it." + field + " Between @" + parameter + "1  AND @" + parameter + "2 )");
            //sb.Append(")");
            return sb.ToString();
        }


        public string InQuery(string field, List<object> values, string Condition, string parameter)
        {
            //string query = "";
            string aux = "it." + field + "=@" + parameter;
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            int i = 1;
            foreach (object o in values)
            {
                if (i < values.Count)
                    sb.Append(aux + i.ToString() + Condition);
                else
                    sb.Append(aux + i.ToString());
                i++;
            }
            sb.Append(")");
            return sb.ToString();
        }

        public string OrderByQuery(string OrderType, string[] field)
        {
            StringBuilder sb = new StringBuilder();
            int i = 1;
            foreach (string el in field)
            {
                if (i < field.Count())

                    sb.Append("it." + el + " " + OrderType + ",");
                else
                    sb.Append("it." + el + " " + OrderType);
                i++;
            }
            return sb.ToString();
        }

        public string SubQuery(string field, SQLOperator EnumOperator, string parameter)
        {
            string SQL = "";
            string Operator = EnumToString(EnumOperator);
            switch (EnumOperator)
            {
                case SQLOperator.Equal:
                case SQLOperator.GreaterThan:
                case SQLOperator.GreaterThanEqual:
                case SQLOperator.LessThan:
                case SQLOperator.LessThanEqual:
                    SQL = field +  Operator + "  @" + parameter;
                    break;
                case SQLOperator.StartWith:
                    SQL = field + Operator + "  @" + parameter + "+'%'";
                    break;
                case SQLOperator.Like:
                    SQL = field + Operator + "  '%'+@" + parameter +"+'%'";
                    break;
                case SQLOperator.EndWith:
                    SQL = field + Operator + "  '%'+@" + parameter ;
                    break;
            }
            return SQL;
        }

        public QueryElement ElementForQuery(List<MethodArgument> ListArgument, string[] OrderByField, string OrderByType)
        {
            QueryElement qe = new QueryElement();
            qe.Query = "";
            qe.ParameterList = new List<ObjectParameter>();
            string ParameterName = "";
            StringBuilder sb = new StringBuilder();
            //Utilities ut = new Utilities();
            if (ListArgument.Count > 0)
            {
                foreach (MethodArgument argument in ListArgument)
                {
                    if (argument.Field != null)
                    {


                        switch (argument.SQLOperator)
                        {
                            case Utilities.SQLOperator.In:
                            case Utilities.SQLOperator.Between:
                                ParameterName = GetParameterName(argument.Field);
                                if (sb.ToString() != "")
                                    //sb.Append("&& " + InQuery(argument.Field, argument.Values, EnumToString(argument.SQLCondition), ParameterName));
                                    sb.Append("&& " + InBetweenQuery(argument.SQLOperator, argument.Field, argument.Values, EnumToString(argument.SQLCondition), ParameterName));
                                else
                                    //sb.Append(InQuery(argument.Field, argument.Values, EnumToString(argument.SQLCondition), ParameterName));
                                    sb.Append(InBetweenQuery(argument.SQLOperator, argument.Field, argument.Values, EnumToString(argument.SQLCondition), ParameterName));
                                int j = 1;
                                foreach (object o in argument.Values)
                                {
                                    qe.ParameterList.Add(new ObjectParameter(ParameterName + j.ToString(), GetParameter(argument.Values[j - 1], argument.ValueType)));
                                    j++;
                                }
                                break;
                            default:
                                ParameterName = GetParameterName(argument.Field);
                                if (sb.ToString() != "")
                                    sb.Append("&& it." + SubQuery(argument.Field, argument.SQLOperator, ParameterName));
                                else
                                    sb.Append("it." + SubQuery(argument.Field, argument.SQLOperator, ParameterName));
                                    qe.ParameterList.Add(new ObjectParameter(ParameterName, GetParameter(argument.Values[0], argument.ValueType)));
                                break;
                                
                        }

                        #region OldCode
                        //if (argument.SQLOperator != Utilities.SQLOperator.In)
                        //{
                        //    ParameterName = ut.ParameterName(argument.Field);
                        //    if (sb.ToString() != "")
                        //        sb.Append("&& it." + ut.SubQuery(argument.Field, argument.SQLOperator, ParameterName));
                        //    else
                        //        sb.Append("it." + ut.SubQuery(argument.Field, argument.SQLOperator, ParameterName));
                        //    qe.ParameterList.Add(new ObjectParameter(ParameterName, ut.GetParameter(argument.Values[0], argument.ValueType)));
                        //}
                        //else
                        //{
                        //    ParameterName = ut.ParameterName(argument.Field);
                        //    if (sb.ToString() != "")
                        //        sb.Append("&& " + ut.InQuery(argument.Field, argument.Values, ut.EnumToString(argument.SQLCondition), ParameterName));
                        //    else
                        //        sb.Append(ut.InQuery(argument.Field, argument.Values, ut.EnumToString(argument.SQLCondition), ParameterName));
                        //    int j = 1;
                        //    foreach (object o in argument.Values)
                        //    {
                        //        qe.ParameterList.Add(new ObjectParameter(ParameterName + j.ToString(), ut.GetParameter(argument.Values[j - 1], argument.ValueType)));
                        //        j++;
                        //    }
                        //}
                        #endregion 
                    }
                }
                qe.Query = sb.ToString();
               
            }
            if (OrderByField.Count() > 0)
            {
                 qe.OrderBy = OrderByQuery(OrderByType, OrderByField.ToArray());
            }
            return qe;
        }

        public  Func<T, string> GetField<T>(string field)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(field);
            return obj => Convert.ToString(propertyInfo.GetValue(obj, null));
        }

        //public List<T> ListEntitities<T>()
        //{
        //    ObjectFactory.CreateObject<T>();
        //    //List<ObjectFactory.CreateObject<T>()> ListEntitities = new List<ObjectFactory.CreateObject<T>()> {};
        //    List<typeof(T)> ListEntitities  = new List<typeof(T)> { };
        //    return new List<typeof(T)> { }
        //}
        #endregion

        #region Type

        public struct QueryElement
        {
            public string Query;
            public string OrderBy;
            public List<ObjectParameter> ParameterList;
        }

        #endregion

        #region Structure

        public struct String_Struct
        {
            public List<string> List;
            public int TotalCount;
        }

        #endregion

    }
}
