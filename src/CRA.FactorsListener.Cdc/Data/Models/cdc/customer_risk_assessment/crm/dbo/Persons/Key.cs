// ------------------------------------------------------------------------------
// <auto-generated>
//    Generated by avrogen, version 1.10.0.0
//    Changes to this file may cause incorrect behavior and will be lost if code
//    is regenerated
// </auto-generated>
// ------------------------------------------------------------------------------
namespace cdc.customer_risk_assessment.crm.dbo.Persons
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Avro;
	using Avro.Specific;
	
	public partial class Key : ISpecificRecord
	{
		public static Schema _SCHEMA = Avro.Schema.Parse("{\"type\":\"record\",\"name\":\"Key\",\"namespace\":\"cdc.customer_risk_assessment.crm.dbo.P" +
				"ersons\",\"fields\":[{\"name\":\"PersonID\",\"type\":\"int\"}],\"connect.name\":\"cdc.customer" +
				"_risk_assessment.crm.dbo.Persons.Key\"}");
		private int _PersonID;
		public virtual Schema Schema
		{
			get
			{
				return Key._SCHEMA;
			}
		}
		public int PersonID
		{
			get
			{
				return this._PersonID;
			}
			set
			{
				this._PersonID = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.PersonID;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.PersonID = (System.Int32)fieldValue; break;
			default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
