using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scv_Model.Common
{
    public class MethodArgument
    {
        private int _intField;
        private string _field;
        private Utilities.SQLCondition _sqlcondtion;
        private Utilities.SQLOperator _sqloperator;
        private List<object> _values;
        private List<string> _OrderByField;
        private Utilities.OrderBy _orderby;
        private int _pagesize;
        private int _pagenumber;
        private Utilities.ValueType _valuetype;



        public string Field
        {
            //get { return _field; }
            //set { _field = value; }
            get;
            set; 
        }

        public int IntField
        {
            get { return _intField; }
            set { _intField = value; }
        }


        public Utilities.SQLCondition SQLCondition
        {
            get { return _sqlcondtion; }
            set { _sqlcondtion = value; }
        }


        public Utilities.SQLOperator SQLOperator
        {
            get { return _sqloperator; }
            set { _sqloperator = value; }
        }

        public List<object> Values
        {
            get { return _values; }
            set { _values = value; }
        }

        public List<string> OrderByField
        {
            get { return _OrderByField; }
            set { _OrderByField = value; }
        }

        public Utilities.OrderBy OrderBy
        {
            get { return _orderby; }
            set { _orderby = value; }
        }
        public int PageSize
        {
            get { return _pagesize; }
            set { _pagesize = value; }
        }
        public int PageNumber
        {
            get { return _pagenumber; }
            set { _pagenumber = value; }
        }

        public Utilities.ValueType ValueType
        {
            get { return _valuetype; }
            set { _valuetype = value; }
        }

    }
}
