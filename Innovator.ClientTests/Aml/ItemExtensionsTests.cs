﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Innovator.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.Tests
{
  [TestClass()]
  public class ItemExtensionsTests
  {
    [TestMethod()]
    public void ToXmlTest()
    {
      var aml = ElementFactory.Local;
      var res = aml.Result();
      Assert.AreEqual(@"<SOAP-ENV:Body xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/""></SOAP-ENV:Body>", res.ToXml().ToString());

      res = aml.Result("Value");
      Assert.AreEqual(@"<SOAP-ENV:Body xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <Result>Value</Result>
</SOAP-ENV:Body>", res.ToXml().ToString());

      res = aml.Result(aml.Item(aml.Type("Part"), aml.Id("1234")), aml.Item(aml.Type("Part"), aml.Id("4567")));
      Assert.AreEqual(@"<SOAP-ENV:Body xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
  <Result>
    <Item type=""Part"" id=""1234"" />
    <Item type=""Part"" id=""4567"" />
  </Result>
</SOAP-ENV:Body>", res.ToXml().ToString());
    }

    [TestMethod()]
    public void LazyMap_ItemDoesNotExist()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='BF3BF6C4795F431D880E7AF4D68D7A9C'>
  <created_by_id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</created_by_id>
  <id keyed_name='Some Company' type='Company'>BF3BF6C4795F431D880E7AF4D68D7A9C</id>
  <permission_id keyed_name='Company' type='Permission'>A8FC3EC44ED0462B9A32D4564FAC0AD8</permission_id>
  <itemtype>3E71E373FC2940B288760C915120AABE</itemtype>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        FirstName = i.CreatedById().AsItem().Property("first_name").Value,
        PermName = i.PermissionId().AsItem().Property("name").Value,
        KeyedName = i.Property("id").KeyedName().Value,
        Empty = i.OwnedById().Value
      });
      Assert.AreEqual("First", result.FirstName);
      Assert.AreEqual("Company", result.PermName);
      Assert.AreEqual("Some Company", result.KeyedName);
      Assert.AreEqual(null, result.Empty);
    }
    [TestMethod()]
    public void LazyMap_PropertyDoesNotExist()
    {
      var conn = new TestConnection();
      var aml = ElementFactory.Local;
      var item = aml.FromXml(@"<Result><Item type='Company' typeId='3E71E373FC2940B288760C915120AABE' id='BF3BF6C4795F431D880E7AF4D68D7A9C'>
  <created_by_id keyed_name='First Last' type='User'>8227040ABF0A46A8AF06C18ABD3967B3</created_by_id>
  <id keyed_name='Some Company' type='Company'>BF3BF6C4795F431D880E7AF4D68D7A9C</id>
  <itemtype>3E71E373FC2940B288760C915120AABE</itemtype>
</Item></Result>").AssertItem();
      var result = item.LazyMap(conn, i => new
      {
        FirstName = i.CreatedById().AsItem().Property("first_name").Value,
        PermName = i.PermissionId().AsItem().Property("name").Value,
        KeyedName = i.Property("id").KeyedName().Value,
        Empty = i.OwnedById().Value
      });
      Assert.AreEqual("First", result.FirstName);
      Assert.AreEqual(null, result.PermName);
      Assert.AreEqual("Some Company", result.KeyedName);
      Assert.AreEqual(null, result.Empty);
    }
  }
}
