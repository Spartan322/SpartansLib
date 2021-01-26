using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace SpartansLib.Xml
{
    public struct Attribute
    {
        public int Index;
        public string Name;
        public string Value;
    }

    public class ParseEventArg
    {
        public XmlNodeType NodeType { get; internal set; }
        public string Name { get; internal set; }
        public string Value { get; internal set; }
        public ReadOnlyCollection<Attribute>  Attributes { get; internal set; }
        public int Depth { get; internal set; }
        public bool EmptyElement { get; internal set; }
    }

    public class Parser
    {
        public event EventHandler<Parser, XmlException> OnException;
        public event EventHandler<Parser, ParseEventArg> OnNode;
        public event EventHandler<Parser, ParseEventArg> OnElement;
        public event EventHandler<Parser, ParseEventArg> OnEndElement;
        public event EventHandler<Parser, ParseEventArg> OnText;
        public event EventHandler<Parser, ParseEventArg> OnProcessingInstruction;
        public event EventHandler<Parser, ParseEventArg> OnDocType;
        public event EventHandler<Parser, ParseEventArg> OnComment;
        public event EventHandler<Parser, ParseEventArg> OnCData;
        public event EventHandler<Parser, ParseEventArg> OnEntity;
        public event EventHandler<Parser, ParseEventArg> OnEntityReference;
        public event EventHandler<Parser, ParseEventArg> OnEntityReferenceUnresolved;

        protected XmlTextReader reader;

        public Parser(XmlTextReader reader)
        {
            this.reader = reader;
        }

        public Parser(string input)
        {
            reader = new XmlTextReader(new StringReader(input));
        }

        public Parser(Uri filepath)
        {
            reader = new XmlTextReader(filepath.AbsoluteUri);
        }

        private readonly Attribute[] empty = new Attribute[0];
        public void Parse()
        {
            try
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    var arg = new ParseEventArg
                    {
                        NodeType = reader.NodeType,
                        Depth = reader.Depth,
                        Name = reader.Name,
                        EmptyElement = reader.IsEmptyElement
                    };
                    if (reader.HasValue) arg.Value = reader.Value;
                    else arg.Value = null;
                    if (reader.HasAttributes)
                    {
                        var arr = new Attribute[reader.AttributeCount];
                        for (var i = 0; i < reader.AttributeCount; i++)
                        {
                            reader.MoveToAttribute(i);
                            arr[i] = new Attribute { Name = reader.Name, Value = reader.Value, Index = i };
                        }
                        reader.MoveToElement();
                        arg.Attributes = new ReadOnlyCollection<Attribute>(arr);
                    }
                    else arg.Attributes = new ReadOnlyCollection<Attribute>(empty);
                    OnNode?.Invoke(this, arg);
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            OnElement?.Invoke(this, arg);
                            break;
                        case XmlNodeType.CDATA:
                            OnCData?.Invoke(this, arg);
                            break;
                        case XmlNodeType.Comment:
                            OnComment?.Invoke(this, arg);
                            break;
                        case XmlNodeType.DocumentType:
                            OnDocType?.Invoke(this, arg);
                            break;
                        case XmlNodeType.EndElement:
                            OnEndElement?.Invoke(this, arg);
                            break;
                        case XmlNodeType.EntityReference:
                            OnEntityReferenceUnresolved?.Invoke(this, arg);
                            reader.ResolveEntity();
                            reader.Read();
                            arg.Name = reader.Name;
                            arg.Value = reader.Value;
                            arg.Depth = reader.Depth;
                            arg.NodeType = reader.NodeType;
                            OnEntityReference?.Invoke(this, arg);
                            break;
                        case XmlNodeType.ProcessingInstruction:
                            OnProcessingInstruction?.Invoke(this, arg);
                            break;
                        case XmlNodeType.Text:
                            OnText?.Invoke(this, arg);
                            break;
                    }
                }
            }
            catch(XmlException e)
            {
                OnException?.Invoke(this, e);
            }
        }
    }
}
