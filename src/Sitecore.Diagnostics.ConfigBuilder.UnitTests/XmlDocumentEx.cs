#region Usings



#endregion

namespace Sitecore.Diagnostics.ConfigBuilder.UnitTests
{
  using System;
  using System.IO;
  using System.Text;
  using System.Xml;

  #region



  #endregion

  /// <summary>
  ///   The xml document ex.
  /// </summary>
  public class XmlDocumentEx : XmlDocument
  {
    #region Properties

    /// <summary>
    ///   Gets or sets FilePath.
    /// </summary>
    public string FilePath { get; protected set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// The load.
    /// </summary>
    /// <param name="filename">
    /// The filename. 
    /// </param>
    public override sealed void Load(string filename)
    {
      this.FilePath = filename;
      base.Load(filename);
    }

    /// <summary>
    ///   The save.
    /// </summary>
    public void Save()
    {
      this.Save(this.FilePath);
    }

    public new static XmlDocumentEx LoadXml(string xml)
    {
      try
      {
        XmlDocument doc = new XmlDocumentEx();
        doc.LoadXml(xml);
        return (XmlDocumentEx)doc;
      }
      catch (Exception ex)
      {
        return null;
      }
    }

    #endregion

    public bool Exists
    {
      get
      {
        return File.Exists(this.FilePath);
      }
    }

    #region Nested type: FileIsMissingException

    /// <summary>
    ///   The file is missing exception.
    /// </summary>
    public class FileIsMissingException : Exception
    {
      #region Constructors

      /// <summary>
      /// Initializes a new instance of the <see cref="FileIsMissingException"/> class.
      /// </summary>
      /// <param name="message">
      /// The message. 
      /// </param>
      public FileIsMissingException(string message)
        : base(message)
      {
      }

      #endregion
    }

    #endregion

    public static XmlDocumentEx LoadFile(string path)
    {
      if (!File.Exists(path))
      {
        throw new FileIsMissingException("The " + path + " doesn't exists");
      }

      var document = new XmlDocumentEx { FilePath = path };
      document.Load(path);
      return document;
    }




    private string FixNotRootedXpathExpression(string xpathExpr)
    {
        if (xpathExpr[0] == '/')
        {
            return xpathExpr;
        }
        else
        {
            return @"/" + xpathExpr;
        }
    }

    public static string Normalize(string xml)
    {
      var doc = XmlDocumentEx.LoadXml(xml);
      var stringWriter = new StringWriter(new StringBuilder());
      var xmlTextWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented };
      doc.Save(xmlTextWriter);
      return stringWriter.ToString();
    }

  }
}