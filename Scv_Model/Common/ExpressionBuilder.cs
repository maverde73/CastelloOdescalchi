using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Data.Objects;
using System.Collections;
using System.Data;
using System.Data.Entity;
using System.Runtime.Serialization;

namespace Scv_Model.Common
{
    public class ExpressionBuilder<T>
    {
        ParameterExpression pExp = Expression.Parameter(typeof(T), "entity");
        MethodInfo mCountInfo = null;
        MethodInfo mCountInfo2 = null;
        MethodInfo mFirst1 = null;
        MethodInfo mFirst2 = null;


        public ExpressionBuilder()
        {
            MethodInfo[] infos = typeof(Enumerable).GetMethods();

            foreach (MethodInfo info in infos)
            {
                if (info.Name == "Count" && info.GetParameters().Length == 2)
                {
                    mCountInfo = info;
                }
                else if (info.Name == "Count" && info.GetParameters().Length == 1)
                {
                    mCountInfo2 = info;
                }
            }
            infos = typeof(System.Linq.Enumerable).GetMethods();
            foreach (MethodInfo info in infos)
            {
                if (info.Name == "FirstOrDefault" && info.GetParameters().Length == 2)
                {
                    mFirst1 = info;
                }
                else if (info.Name == "FirstOrDefault" && info.GetParameters().Length == 1)
                {
                    mFirst2 = info;
                }
            }
        }

        public Expression<Func<T, bool>> In(string field, object[] value)
        {

            Expression<Func<T, bool>> ret = null;
            foreach (object o in value)
            {
                if (ret == null)
                {
                    ret = Create(field, "=", o);
                }
                else
                {
                    ret = Or(ret, Create(field, "=", o));
                }
            }
            return ret;
        }

        public Expression<Func<T, bool>> InAnd(string field, object[] value)
        {

            Expression<Func<T, bool>> ret = null;
            Expression pEntity = null;
            int i = 0;
            foreach (object o in value)
            {
                if (ret == null)
                {
                    ret = Create(field, "=", o, out pEntity);
                    i++;
                }
                else
                {
                    ret = And(ret, Create(field, "=", o, out pEntity));
                    i++;
                }
            }
            Expression p6 = Expression.Call(pEntity, mCountInfo2.MakeGenericMethod(pEntity.Type.GetGenericArguments()[0]), new Expression[] { pEntity });
            Expression p7 = BinaryExpression.Equal(p6, Expression.Constant(i));
            Expression p8 = Expression.Lambda<Func<T, bool>>(p7, pExp);
            ret = And(ret, (Expression<Func<T, bool>>)p8);
            return ret;
        }



        public Expression<Func<T, bool>> MaxMin(string field, object[] value)
        {

            Expression<Func<T, bool>> ret = null;
            /*
              object[] ha 2 elementi: limite inferiore  e limite superiore 
              object[0] limite inferiore 
              object[1] limite superiore 
             */
            foreach (object o in value)
            {
                if (ret == null)
                {
                    ret = Create(field, ">=", o);
                }
                else
                {
                    ret = And(ret, Create(field, "<=", o));
                }
            }
            return ret;
        }

        public Expression<Func<T, bool>> Create(string fields, string expression, object value)
        {
            Expression ret = null;
            return Create(fields, expression, value, out ret);
        }
        private Expression<Func<T, bool>> Create(string fields, string expression, object value, out Expression pEntity)
        {
            string[] field = fields.Split('.');
            BinaryExpression bExp = null;
            Expression member = Expression.PropertyOrField(pExp, field[0]);
            ConstantExpression cExp = Expression.Constant(value);
            ParameterExpression p1 = null;
            //Expression pEntity = null;
            pEntity = null;
            Type genType = null;
            for (int i = 1; i < field.Length; i++)
            {
                if ((member.Type.GetInterface("IEnumerable") == null))
                {
                    member = Expression.PropertyOrField(member, field[i]);
                }
                else
                {
                    //member<p1>
                    genType = member.Type;
                    p1 = Expression.Parameter(member.Type.GetGenericArguments()[0], genType.GetGenericArguments()[0].Name);
                    pEntity = member;
                    member = Expression.PropertyOrField(p1, field[i]);
                }

            }
            if (p1 != null)
            {
                Expression p4 = createExpression(member, expression, value);
                Expression p5 = Expression.Lambda(p4, new ParameterExpression[] { p1 });
                Expression p6 = Expression.Call(pEntity, mCountInfo.MakeGenericMethod(pEntity.Type.GetGenericArguments()[0]), new Expression[] { pEntity, p5 });
                Expression p7 = BinaryExpression.GreaterThan(p6, Expression.Constant(0));
                return Expression.Lambda<Func<T, bool>>(p7, pExp);
            }
            else
            {
                bExp = createExpression(member, expression, value);
                return Expression.Lambda<Func<T, bool>>(bExp, pExp);
            }
        }
        public Expression GetOrderExpressionMember(string fields)
        {
            string[] field = fields.Split('.');
            Expression member = Expression.PropertyOrField(pExp, field[0]);
            ParameterExpression p1 = null;
            Type genType = null;
            for (int i = 1; i < field.Length; i++)
            {
                if ((member.Type.GetInterface("IEnumerable") == null))
                {
                    member = Expression.PropertyOrField(member, field[i]);
                }
                else
                {
                    genType = member.Type;
                    p1 = Expression.Parameter(member.Type.GetGenericArguments()[0], genType.GetGenericArguments()[0].Name);
                    member = Expression.Call(member, mFirst2.MakeGenericMethod(member.Type.GetGenericArguments()[0]), new Expression[] { member/*, member*/ });
                    member = Expression.PropertyOrField(member, field[i]);
                }

            }
            return member;
        }

        private Expression getValue(Expression member)
        {
            if (member.Type.Name.StartsWith("Nullable"))
            {
                member = Expression.PropertyOrField(member, "Value");
            }
            return member;
        }

        //public Expression<Func<T, bool>> Create(string fields, string expression, object value)
        //{
        //    string[] field = fields.Split('.');
        //    BinaryExpression bExp = null;
        //    Expression member = Expression.PropertyOrField(pExp, field[0]);
        //    //***********
        //   // Expression member1 =null;
        //    //if(field.Length>2)
        //    //    member1 = Expression.PropertyOrField(pExp, field[1]);
        //    //**************
        //    ConstantExpression cExp = Expression.Constant(value);
        //    for (int i = 1; i < field.Length; i++)
        //    {
        //        if ((member.Type.GetInterface("IEnumerable") == null))
        //        {
        //            member = Expression.PropertyOrField(member, field[i]);
        //        }
        //        else
        //        {
        //            MethodInfo[] infos = typeof(Enumerable).GetMethods();
        //            MethodInfo minfo = null;
        //            foreach (MethodInfo info in infos)
        //            {
        //                if (info.Name == "Count" && info.GetParameters().Length == 2)
        //                {
        //                    minfo = info;
        //                    break;
        //                }
        //            }
        //            ParameterExpression p1 = Expression.Parameter(member.Type.GetGenericArguments()[0], member.Type.GetGenericArguments()[0].Name);
        //            //***************
        //            //ParameterExpression ptest = Expression.Parameter(member1.Type.GetGenericArguments()[0], member1.Type.GetGenericArguments()[0].Name);
        //            //Expression expTest = Expression.PropertyOrField(ptest, field[i + 1]);
        //            //***************
        //            Expression p2 = Expression.PropertyOrField(p1, field[i]);
        //            Expression p3 = Expression.PropertyOrField(p2, field[i + 1]);
        //            Expression p4 = createExpression(p3, expression, value);// ParameterExpression.Equal(p3, cExp);
        //            Expression p5 = Expression.Lambda(p4, new ParameterExpression[] { p1 });
        //            member = Expression.Call(member, minfo.MakeGenericMethod(member.Type.GetGenericArguments()[0]), new Expression[] { member, p5 });
        //            Expression p6 = BinaryExpression.GreaterThan(member, Expression.Constant(0));
        //            return Expression.Lambda<Func<T, bool>>(p6, pExp);
        //        }

        //    }
        //    bExp = createExpression(member, expression, value);
        //    return Expression.Lambda<Func<T, bool>>(bExp, pExp);
        //}



        private BinaryExpression createExpression(Expression member, string expression, object value)
        {
            member = getValue(member);
            ConstantExpression cExp = Expression.Constant(Convert.ChangeType(value, member.Type));
            switch (expression.ToLower())
            {
                case "=":
                    return BinaryExpression.Equal(member, cExp);
                case ">=":
                    return BinaryExpression.GreaterThanOrEqual(member, cExp);
                case "<=":
                    return BinaryExpression.LessThanOrEqual(member, cExp);
                case ">":
                    return BinaryExpression.GreaterThan(member, cExp);
                case "<":
                    return BinaryExpression.LessThan(member, cExp);
                case "!=":
                    return BinaryExpression.NotEqual(member, cExp);
                case "like":


                    Expression like = BinaryExpression.Call(member, (typeof(string)).GetMethod("Contains"), new Expression[] { cExp });
                    return BinaryExpression.Equal(like, Expression.Constant(true));
                case "startwith":
                    Expression start = BinaryExpression.Call(member, (typeof(string)).GetMethod("StartsWith", new Type[] { typeof(string) }), new Expression[] { cExp });
                    return BinaryExpression.Equal(start, Expression.Constant(true));
                case "endwith":
                    Expression end = BinaryExpression.Call(member, (typeof(string)).GetMethod("EndsWith", new Type[] { typeof(string) }), new Expression[] { cExp });
                    return BinaryExpression.Equal(end, Expression.Constant(true));
                default:
                    return null;
            }

        }
        public static bool Like(string s, string val)
        {
            if (s.IndexOf('%') == 0 && s[s.Length - 1] == '%')
            {
                return s.Replace("%", "").ToUpper().Contains(val.ToUpper());
            }
            else if (s.IndexOf('%') == 0)
            {
                return s.Replace("%", "").ToUpper().StartsWith(val.ToUpper());
            }
            else if (s[s.Length - 1] == '%')
            {
                return s.Replace("%", "").ToUpper().EndsWith(val.ToUpper());
            }
            else
            {
                return s.ToUpper() == val.ToUpper();
            }
        }
        public Expression<Func<T, bool>> And(Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
        {
            var ez = MemberExpression.And(e1.Body, e2.Body);
            var ret = Expression.Lambda<Func<T, bool>>(ez, new ParameterExpression[] { pExp });
            return ret;
        }
        public Expression<Func<T, bool>> Or(Expression<Func<T, bool>> e1, Expression<Func<T, bool>> e2)
        {
            var ez = MemberExpression.Or(e1.Body, e2.Body);
            var ret = Expression.Lambda<Func<T, bool>>(ez, new ParameterExpression[] { pExp });
            return ret;
        }



        public Expression<Func<T, bool>> WhereExpression(List<MethodArgument> ListArgument)
        {
            ExpressionBuilder<T> eb = new ExpressionBuilder<T>();
            Expression<Func<T, bool>> exp = null;
            Expression<Func<T, bool>> expAux = null;

            Utilities ut = new Utilities();

            List<Expression<Func<T, bool>>> ExpressionList = new List<Expression<Func<T, bool>>> { };

            if (ListArgument.Count > 0)
            {
                foreach (MethodArgument argument in ListArgument)
                {
                    if (argument.Field != null)
                    {
                        switch (argument.SQLOperator)
                        {
                            case Utilities.SQLOperator.In:
                                //case Utilities.SQLOperator.Between:
                                if (argument.SQLCondition == Utilities.SQLCondition.And)
                                    expAux = eb.InAnd(argument.Field, argument.Values.ToArray());
                                else
                                    expAux = eb.In(argument.Field, argument.Values.ToArray());
                                ExpressionList.Add(expAux);
                                break;
                            case Utilities.SQLOperator.Between:
                                expAux = eb.MaxMin(argument.Field, argument.Values.ToArray());
                                ExpressionList.Add(expAux);
                                break;
                            default:
                                //expAux = eb.Create(argument.Field, ut.EnumToString(argument.SQLOperator).Trim(), ut.GetParameter(argument.Values[0], argument.ValueType));
                                expAux = eb.Create(argument.Field, ut.EnumToString(argument.SQLOperator).Trim(), argument.Values[0]);
                                ExpressionList.Add(expAux);
                                break;
                        }
                    }
                }
            }
            foreach (Expression<Func<T, bool>> le in ExpressionList)
            {
                if (exp == null)
                    exp = le;
                else
                    exp = eb.And(exp, le);
            }
            return exp;
        }

        //public IOrderedQueryable<T> Apply(IQueryable<T> source)
        //{
        //    if (function == null)
        //    {
        //        throw new InvalidOperationException("No ordering defined");
        //    }
        //    return function(source);
        //}

        public Expression<Func<T, TKey>> OrderExpression<TKey>(string field)
        {

            Expression member = Expression.PropertyOrField(pExp, field);
            System.Type pType = member.Type;
            member = getValue(member);
            Expression<Func<T, TKey>> expOrd = Expression.Lambda<Func<T, TKey>>(member, pExp);
            //return Expression.Lambda<Func<T, TKey>>(member, pExp);
            return expOrd;
        }
        public Expression<Func<T, TKey>> OrderExpression<TKey>(Expression member, TKey val) where TKey : class
        {
            //Expression member = Expression.PropertyOrField(pExp, field);
            //System.Type pType = member.Type;

            //member = getValue(member);
            Expression<Func<T, TKey>> expOrd = Expression.Lambda<Func<T, TKey>>(member, pExp);
            //return Expression.Lambda<Func<T, TKey>>(member, pExp);
            return expOrd;
        }

        private ValueType getObject(Type t)
        {

            //if (t.Name.StartsWith("Nullable"))
            //{
            //    return Activator.CreateInstance(t);
            //}
            if (!t.IsClass)
            {
                if (t.Name == "DateTime")
                {
                    return DateTime.Now;
                }
                else
                {
                    return (ValueType)t.GetMethod("Parse", new Type[] { typeof(string) }).Invoke(null, new object[] { "1" });
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public Expression<Func<T, TKey>> OrderExpression<TKey>(Expression member)
        {
            //Expression member = Expression.PropertyOrField(pExp, field);
            //System.Type pType = member.Type;

            //member = getValue(member);

            //return Expression.Lambda<Func<T, TKey>>(Expression.Lambda<Func<T, object>>(member, pExp));
            return Expression.Lambda<Func<T, TKey>>(member, pExp);
            //return Expression.Lambda<Func<T, TKey>>(member, pExp);
            //return expOrd;
        }
        public Expression<Func<T, TKey>> OrderExpression2<TKey>(Expression member)
        {
            //Expression member = Expression.PropertyOrField(pExp, field);
            //System.Type pType = member.Type;

            //member = getValue(member);

            //return Expression.Lambda<Func<T, TKey>>(Expression.Lambda<Func<T, object>>(member, pExp));
            return Expression.Lambda<Func<T, TKey>>(member, pExp);
            //return Expression.Lambda<Func<T, TKey>>(member, pExp);
            //return expOrd;
        }
        public Expression<Func<T, System.Type>> OrderExpressionTest<TKey>(Expression member)
        {
            //Expression member = Expression.PropertyOrField(pExp, field);
            //System.Type pType = member.Type;
            member = getValue(member);
            Expression<Func<T, System.Type>> expOrd = Expression.Lambda<Func<T, System.Type>>(member, pExp);
            //return Expression.Lambda<Func<T, TKey>>(member, pExp);
            return expOrd;
        }


        public IOrderedQueryable<T> OrderedQuery(IOrderedQueryable<T> Query, string[] OrderByField, string OrderByType)
        {
            //var orderQuery ;
            for (int j = 0; j < OrderByField.Length; j++)
            {

                Expression member = GetOrderExpressionMember(OrderByField[j]);
                #region Codice da eliminare
                //string[] field = OrderByField[j].Split('.');
                //Expression member = Expression.PropertyOrField(pExp, field[0]);
                //ParameterExpression p1 = null;
                //Type genType = null;
                //for (int i = 1; i < field.Length; i++)
                //{
                //    if ((member.Type.GetInterface("IEnumerable") == null))
                //    {
                //        member = Expression.PropertyOrField(member, field[i]);
                //    }
                //    else
                //    {
                //        genType = member.Type;
                //        p1 = Expression.Parameter(member.Type.GetGenericArguments()[0], genType.GetGenericArguments()[0].Name);
                //        member = Expression.PropertyOrField(p1, field[i]);
                //    }
                //}
                #endregion

                //richiamo il metodo per creare la order by da aggiungere 
                //var orderQuery = OrderExpression(member,getObject(member.Type));


                //MethodInfo method;
                //if (j == 0)
                //{
                //    if (OrderByType == "ASC")
                //    {
                //        method = Query.GetType().GetMethod("OrderBy", new Type[] { this.GetType().GetMethod("OrderExpression2").ReturnType });
                //    }
                //    else
                //    {
                //        method = Query.GetType().GetMethod("OrderByDescending", new Type[] { this.GetType().GetMethod("OrderExpression2").ReturnType });
                //    }
                //}
                //else
                //{
                //    if (OrderByType == "ASC")
                //    {
                //        method = Query.GetType().GetMethod("ThenBy", new Type[] { this.GetType().GetMethod("OrderExpression2").ReturnType });
                //    }
                //    else
                //    {
                //        method = Query.GetType().GetMethod("ThenByDescending", new Type[] { this.GetType().GetMethod("OrderExpression2").ReturnType });
                //    }
                //}

                //MethodInfo OrderBy = typeof(System.Linq.Enumerable).GetMethods().First(a => a.Name == "OrderBy" && a.GetParameters().Length == 2 && a.GetParameters()[0].ParameterType.Name.StartsWith("IEnumerable"));
                //MethodInfo OrderByDescending = typeof(System.Linq.Enumerable).GetMethods().First(a => a.Name == "OrderByDescending" && a.GetParameters().Length == 2 && a.GetParameters()[0].ParameterType.Name.StartsWith("IEnumerable"));
                //MethodInfo ThenBy = typeof(System.Linq.Enumerable).GetMethods().First(a => a.Name == "ThenBy" && a.GetParameters().Length == 2 && a.GetParameters()[0].ParameterType.Name.StartsWith("IEnumerable"));
                //MethodInfo ThenByDescending = typeof(System.Linq.Enumerable).GetMethods().First(a => a.Name == "ThenByDescending" && a.GetParameters().Length == 2 && a.GetParameters()[0].ParameterType.Name.StartsWith("IEnumerable"));


                //******aggiunta Flavio
                member = getValue(member);
                //***fine aggiounta Flavio***
                if (j == 0)
                {
                    switch (member.Type.Name.ToUpper())
                    {
                        case "STRING":
                            {
                                var orderQuery = OrderExpression2<String>(member);
                                //Query = (IOrderedQueryable<T>) method.Invoke(Query, new object[] { orderQuery });
                                if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }
						case "INT16":
						case "INT32":
						case "INT64":
							{
                                var orderQuery = OrderExpression2<Int32>(member);
                                if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }


                        case "DOUBLE":
                            {
                                var orderQuery = OrderExpression2<Double>(member);
                                if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }
                        case "DATETIME":
                            {
                                var orderQuery = OrderExpression2<DateTime>(member);
                                if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }

						case "BOOLEAN":
						case "BOOL":
							{
                                var orderQuery = OrderExpression2<Boolean>(member);
                                if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }
                        default:
                            break;
                    }

                }
                else
                {
                    switch (member.Type.Name.ToUpper())
                    {
                        case "STRING":
                            {
                                var orderQuery = OrderExpression2<String>(member);
                                if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.ThenBy(orderQuery);
                                else
                                    Query = Query.ThenByDescending(orderQuery);
                                break;
                            }
						case "INT16":
						case "INT32":
						case "INT64":
							{
                                var orderQuery = OrderExpression2<Int32>(member);
								if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.ThenBy(orderQuery);
                                else
                                    Query = Query.ThenByDescending(orderQuery);
                                break;
                            }
                        case "DOUBLE":
                            {
                                var orderQuery = OrderExpression2<Double>(member);
								if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.ThenBy(orderQuery);
                                else
                                    Query = Query.ThenByDescending(orderQuery);
                                break;
                            }
                        case "DATETIME":
                            {
                                var orderQuery = OrderExpression2<DateTime>(member);
								if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.ThenBy(orderQuery);
                                else
                                    Query = Query.ThenByDescending(orderQuery);
                                break;
                            }

						case "BOOLEAN":
						case "BOOL":
							{
                                var orderQuery = OrderExpression2<Boolean>(member);
								if (OrderByType.ToUpper() == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }
                        default:
                            break;
                    }
                }
                //***********************************************************
                //Expression orderQuery;// = OrderExpression(member);
                //// devo passare il tipo corretto al metodo OrderExpression
                ////var orderQuery;
                //if (!member.Type.IsClass)
                //{
                //    if (member.Type.Name == "DateTime")
                //    {
                //        orderQuery = OrderExpression<DateTime>(member);
                //    }
                //    else
                //    {
                //        orderQuery = OrderExpression(member,member.Type.GetMethod("Parse",new Type[]{typeof(string)}).Invoke(null,new object[]{"1"}));
                //    }
                //}
                //else
                //{
                //    orderQuery = OrderExpression<System.String>(member);
                //}
                //************************************************



                //switch (member.Type.Name)
                //{
                //    case "String":
                //        orderQuery = OrderExpression<System.String>(member);
                //        break;
                //    case "Int32":
                //        orderQuery = OrderExpression<System.Int32>(member);
                //        break;
                //}


                //OrderExpressionTest<member.Type>(member);
                // con la function creata costruisco la OrderBy

            }


            return Query;
        }

        /*l'ordinamento prima era solo o AScendente o discendente nel caso del report della matrice di letteratura l'ordinamento
           deve essere ascendente per attività economica e sede anatomica mentre deve essere discendente per anno
        */
        public IOrderedQueryable<T> OrderedQuery(IOrderedQueryable<T> Query, string[] OrderByField, string[] OrderByType)
        {
            //var orderQuery ;
            for (int j = 0; j < OrderByField.Length; j++)
            {

                Expression member = GetOrderExpressionMember(OrderByField[j]);





                //******aggiunta Flavio
                member = getValue(member);
                //***fine aggiounta Flavio***
                if (j == 0)
                {
                    switch (member.Type.Name)
                    {
                        case "String":
                            {
                                var orderQuery = OrderExpression2<String>(member);
                                //Query = (IOrderedQueryable<T>) method.Invoke(Query, new object[] { orderQuery });
                                if (OrderByType[0] == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }
                        case "Int32":
                            {
                                var orderQuery = OrderExpression2<Int32>(member);
                                if (OrderByType[0] == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }


                        case "Double":
                            {
                                var orderQuery = OrderExpression2<Double>(member);
                                if (OrderByType[0] == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }
                        case "DateTime":
                            {
                                var orderQuery = OrderExpression2<DateTime>(member);
                                if (OrderByType[0] == "ASC")
                                    Query = Query.OrderBy(orderQuery);
                                else
                                    Query = Query.OrderByDescending(orderQuery);
                                break;
                            }
                        default:
                            break;
                    }

                }
                else
                {
                    switch (member.Type.Name)
                    {
                        case "String":
                            {
                                var orderQuery = OrderExpression2<String>(member);
                                if (OrderByType[j] == "ASC")
                                    Query = Query.ThenBy(orderQuery);
                                else
                                    Query = Query.ThenByDescending(orderQuery);
                                break;
                            }
                        case "Int32":
                            {
                                var orderQuery = OrderExpression2<Int32>(member);
                                if (OrderByType[j] == "ASC")
                                    Query = Query.ThenBy(orderQuery);
                                else
                                    Query = Query.ThenByDescending(orderQuery);
                                break;
                            }
                        case "Double":
                            {
                                var orderQuery = OrderExpression2<Double>(member);
                                if (OrderByType[j] == "ASC")
                                    Query = Query.ThenBy(orderQuery);
                                else
                                    Query = Query.ThenByDescending(orderQuery);
                                break;
                            }
                        case "DateTime":
                            {
                                var orderQuery = OrderExpression2<DateTime>(member);
                                if (OrderByType[j] == "ASC")
                                    Query = Query.ThenBy(orderQuery);
                                else
                                    Query = Query.ThenByDescending(orderQuery);
                                break;
                            }
                        default:
                            break;
                    }
                }


            }


            return Query;
        }

        //public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> input, string queryString)
        //{
        //    if (string.IsNullOrEmpty(queryString))
        //        return input;

        //    int i = 0;
        //    foreach (string propname in queryString.Split(','))
        //    {
        //        var subContent = propname.Split('|');
        //        if (Convert.ToInt32(subContent[1].Trim()) == 0)
        //        {
        //            if (i == 0)
        //                input = input.OrderBy(x => GetPropertyValue(x, subContent[0].Trim()));
        //            else
        //                input = ((IOrderedEnumerable<T>)input).ThenBy(x => GetPropertyValue(x, subContent[0].Trim()));
        //        }
        //        else
        //        {
        //            if (i == 0)
        //                input = input.OrderByDescending(x => GetPropertyValue(x, subContent[0].Trim()));
        //            else
        //                input = ((IOrderedEnumerable<T>)input).ThenByDescending(x => GetPropertyValue(x, subContent[0].Trim()));
        //        }
        //        i++;
        //    }

        //    return input;
        //} 

    }

}

