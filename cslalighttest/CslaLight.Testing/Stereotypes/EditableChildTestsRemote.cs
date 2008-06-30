﻿using cslalighttest.Engine;
using Csla;
using Csla.Testing.Business.EditableChildTests;
using cslalighttest.Properties;
using System;

namespace cslalighttest.Stereotypes
{
  [TestClass]
  public class EditableChildTestsRemote
  {
    [TestSetup]
    public void Setup()
    {
      Csla.DataPortal.ProxyTypeName = "Csla.DataPortalClient.WcfProxy, Csla";
      Csla.DataPortalClient.WcfProxy.DefaultUrl = Resources.RemotePortalUrl;
    }

    [TestMethod]
    public void ListWithChildrenNoCriteria(AsyncTestContext context)
    {
      MockList.FetchAll((l, e) =>
      {
        context.Assert.IsNull(e);
        context.Assert.IsNotNull(l);
        context.Assert.AreEqual(3, l.Count);

        context.Assert.AreEqual(MockList.MockEditableChildId1, l[0].Id);
        context.Assert.AreEqual("Child_Fetch", l[0].DataPortalMethod);
        context.Assert.IsNotNull(l[0].GrandChildren);
        context.Assert.AreEqual(1, l[0].GrandChildren.Count);
        context.Assert.AreEqual(GrandChildList.GrandChildId1, l[0].GrandChildren[0].Id);
        context.Assert.AreEqual("Child_Fetch", l[0].GrandChildren[0].DataPortalMethod);

        context.Assert.AreEqual(MockList.MockEditableChildId2, l[1].Id);
        context.Assert.AreEqual("Child_Fetch", l[1].DataPortalMethod);
        context.Assert.IsNotNull(l[1].GrandChildren);
        context.Assert.AreEqual(1, l[1].GrandChildren.Count);
        context.Assert.AreEqual(GrandChildList.GrandChildId2, l[1].GrandChildren[0].Id);
        context.Assert.AreEqual("Child_Fetch", l[1].GrandChildren[0].DataPortalMethod);

        context.Assert.AreEqual(MockList.MockEditableChildId3, l[2].Id);
        context.Assert.AreEqual("Child_Fetch", l[2].DataPortalMethod);
        context.Assert.IsNotNull(l[2].GrandChildren);
        context.Assert.AreEqual(1, l[2].GrandChildren.Count);
        context.Assert.AreEqual(GrandChildList.GrandChildId3, l[2].GrandChildren[0].Id);
        context.Assert.AreEqual("Child_Fetch", l[2].GrandChildren[0].DataPortalMethod);

        context.Assert.Success();
      });
    }

    [TestMethod]
    public void ListWithChildrenCriteria(AsyncTestContext context)
    {
      MockList.FetchByName("c2", (l, e) =>
      {
        context.Assert.IsNull(e);
        context.Assert.IsNotNull(l);
        context.Assert.AreEqual(1, l.Count);
        context.Assert.AreEqual(MockList.MockEditableChildId2, l[0].Id);
        Assert.AreEqual("Child_Fetch", l[0].DataPortalMethod);
        context.Assert.IsFalse(l[0].IsNew);
        context.Assert.IsFalse(l[0].IsDirty);

        context.Assert.AreEqual(1, l[0].GrandChildren.Count);
        context.Assert.AreEqual(GrandChildList.GrandChildId2, l[0].GrandChildren[0].Id);
        Assert.AreEqual("Child_Fetch", l[0].GrandChildren[0].DataPortalMethod);
        context.Assert.IsFalse(l[0].GrandChildren[0].IsNew);
        context.Assert.IsFalse(l[0].GrandChildren[0].IsDirty);

        context.Assert.Success();
      });
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void SaveChildFail(AsyncTestContext context)
    {
      MockList.FetchByName("c2", (l, e) => 
      {
        context.Assert.IsNull(e);
        context.Assert.IsNotNull(l);
        context.Assert.Try(() => l[0].Save());
      });
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void SaveGrandChildListFail(AsyncTestContext context)
    {
      MockList.FetchByName("c2", (l, e) => 
      {
        context.Assert.IsNull(e);
        context.Assert.IsNotNull(l);
        context.Assert.Try(()=> l[0].GrandChildren.Save());
      });
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void SaveGrandChildFail(AsyncTestContext context)
    {
      MockList.FetchByName("c2", (l, e) => 
      {
        context.Assert.IsNull(e);
        context.Assert.IsNotNull(l);
        context.Assert.Try(() => l[0].GrandChildren[0].Save());
      });
    }

    [TestMethod]
    public void UpdateMockEditableChild(AsyncTestContext context)
    {
      MockList.FetchByName("c2", (l, e) =>
      {
        context.Assert.IsNull(e);
        context.Assert.IsNotNull(l);
        context.Assert.AreEqual(1, l.Count);
        context.Assert.AreEqual("c2", l[0].Name);

        l[0].Name = "saving";
        l.Saved += (o, e2) =>
        {
          context.Assert.IsNotNull(e2.NewObject);
          MockList l2 = (MockList)e2.NewObject;

          context.Assert.AreEqual(l.Count, l2.Count);
          context.Assert.AreEqual(l[0].Id, l2[0].Id);
          context.Assert.AreEqual(l[0].Name, l2[0].Name);

          context.Assert.AreEqual("Child_Fetch", l[0].DataPortalMethod);
          context.Assert.AreEqual("Child_Update", l2[0].DataPortalMethod);

          context.Assert.IsTrue(l[0].IsDirty);
          context.Assert.IsFalse(l2[0].IsDirty);

          context.Assert.AreEqual(l[0].GrandChildren.Count, l2[0].GrandChildren.Count);

          l2[0].GrandChildren[0].Name = "saving";
          l2.Saved += (o2, e3) =>
          {
            MockList l3 = (MockList)e3.NewObject;
            context.Assert.AreEqual("Child_Fetch", l2[0].GrandChildren[0].DataPortalMethod);
            context.Assert.IsTrue(l2[0].GrandChildren[0].IsDirty);
            context.Assert.AreEqual("Child_Update", l3[0].GrandChildren[0].DataPortalMethod);
            context.Assert.IsFalse(l3[0].GrandChildren[0].IsDirty);

            context.Assert.Success();
          };
          l2.Save();
        };
        l.Save();
      });
    }
  }
}