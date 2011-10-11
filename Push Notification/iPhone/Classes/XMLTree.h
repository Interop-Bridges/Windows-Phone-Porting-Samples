#import <CoreFoundation/CoreFoundation.h>

/*!
 @header XMLTree
 XMLTree provides an Objective-C wrapper for Apple's built-in C-language
 XML parser and manipulation functions.
 */


/*!
 @class XMLTree
 @abstract Wraps some C-level functions from Apple for XML manipulation.
 @discussion
 <p>
 XMLTree provides an Objective-C wrapper for Apple's built-in C-language
 XML parser and manipulation functions.
 At the moment it only supports basic element and attribute information.
 However Apple's XML parser supports processing instructions, CDATA,
 and some other things I've never seen, so I'll add support for these
 as I go along.
 </p>
 <p>
 I'm releasing this code into the Public Domain, so you can include it
 with your software regardless of the license you use. If you make any
 useful additions or bug fixes (especially with retain/release), we
 would all appreciate it if you would let me know so we can give the
 changes to everyone else too.
 </p>
 <p>Author: Robert Harder, rob -at- iharder.net</p>
 <p>version: 0.1</p>
 */
@interface XMLTree : NSObject
{
    CFTreeRef _tree;
    CFXMLNodeRef _node;

    NSString    *_lastUnknownSelector;
}


/*!
 @method dealloc
 @abstract Be a good citizen and clean up after ourselves.
 */
-(void)dealloc;


/*!
 @method treeWithURL:
 @abstract Creates an autoreleased XMLTree with the contents of <var>url</var>.
 @discussion
 Creates an autoreleased XMLTree with the contents of <var>url</var> or
 <tt>nil</tt> if there was an error.
 Of course the URL can be pointing to a file or a URL on the internet
 such as a GET command to a SOAP application.
 @param url The <tt>NSURL</tt> pointing to your XML data.
 @result An autoreleased <tt>XMLTree</tt> with the contents
 of <var>url</var> or <tt>nil</tt> if there was a problem.
 */
+(XMLTree *)treeWithURL:(NSURL *)url;




/*!
 @method init
 @abstract Initializes and returns an <tt>XMLTree</tt>.
 @discussion
  Initializes and returns an <tt>XMLTree</tt> (with a retain count of 1).
  There isn't much point to creating an <tt>XMLTree</tt> this way until
  I add methods for manuallying adding XML nodes to the tree.
 @result An <tt>XMLTree</tt> (with a retain count of 1).
 */
-(XMLTree *)init;




/*!
 @method initWithURL:
 @abstract Initializes and returns an <tt>XMLTree</tt>
 with the contents of <var>url</var>.
 @discussion
  Initializes and returns an <tt>XMLTree</tt> (with a retain count of 1)
  with the XML contents of <var>url</var> or <tt>nil</tt> if there is an error.
 @param url The <tt>NSURL</tt> pointing to your XML data.
 @result An <tt>XMLTree</tt> with a retain count of 1.
 */
-(XMLTree *)initWithURL:(NSURL *)url;






/*!
 @method initWithCFXMLTreeRef:
 @abstract Initializes and returns an <tt>XMLTree</tt>
 with the internal data represented by <var>ref</var>.
 @discussion
  Initializes and returns an <tt>XMLTree</tt> (with a retain count of 1)
  with the internal <tt>CFXMLTreeRef</tt> data represented by <var>ref</var>.
  You probably won't ever need to call this yourself, but I call it internally
  and may move it to a Private API in the XMLTree.m file later.
 @param ref The <tt>CFXMLTreeRef</tt> containing the XML data.
 @result An <tt>XMLTree</tt> with a retain count of 1.
 */
-(XMLTree *)initWithCFXMLTreeRef:(CFXMLTreeRef)ref;



    /*!
     @method initWithData:withResolvingURL:
     @abstract Initializes and returns an <tt>XMLTree</tt>
     with the XML data stored in <var>inData</var> and
     an optional URL used to resolve references.
     @discussion
     Initializes and returns an <tt>XMLTree</tt> (with a retain count of 1)
     with the internal <tt>NSData</tt> data represented by <var>inData</var>.
     The <tt>NSURL</tt> url is optional. If provided, Apple's XML parser
     may use it to resolve links in the XML code.
     @param inData The <tt>NSData</tt> containing the XML data.
     @param url The <tt>NSURL</tt> for resolving links.
     @result An <tt>XMLTree</tt> with a retain count of 1.
     */
- (XMLTree *)initWithData:(NSData *)inData withResolvingURL:(NSURL *)url;


    /*!
     @method initWithData:
     @abstract Initializes and returns an <tt>XMLTree</tt>
     with the XML data stored in <var>inData</var>.
     @discussion
     Initializes and returns an <tt>XMLTree</tt> (with a retain count of 1)
     with the internal <tt>NSData</tt> data represented by <var>inData</var>.
     @param inData The <tt>NSData</tt> containing the XML data.
     @result An <tt>XMLTree</tt> with a retain count of 1.
     */
- (XMLTree *)initWithData:(NSData *)inData;


/* ********  A B O U T   S E L F ******** */


/*!
 @method name
 @abstract Returns the name of the root node in the tree.
 @discussion
 Returns the name of the root node in the tree or <tt>nil</tt>
 if a name is not appropriate in the current context such as
 if the "tree" is actually a single XML Processing Instruction node.
 @result The name of the root node in the tree..
 */
-(NSString *)name;


/*!
 @method xml
 @abstract Returns the <tt>XMLTree</tt> in an XML-looking form.
 @discussion
 Returns the <tt>XMLTree</tt> in an XML-looking form as performed
 by Apple's own <tt>CFXMLTreeCreateXMLData(...)</tt> method.
 @result The <tt>XMLTree</tt> in an XML-looking form.
 */
-(NSString *)xml;



/*!
 @method description
 @abstract Returns a textual representation of the <tt>XMLTree</tt>.
 @discussion
 <p>
  Returns a textual representation of the <tt>XMLTree</tt>.
  The way the tree is interpreted depends on what kind of root
  node is represented by the receiver.
 </p>
 <p>
  Listed below are the actions this method takes depending
  on the type of node this is.
 </p>
 <table border="1">
   <thead>
    <tr>
     <th>Node Type</th><th>CFXMLNodeTypeCode</th><th>Action</th>
    </tr>
   </thead>
   <tbody>
    <tr>
     <td>Document</td><td>kCFXMLNodeTypeDocument</td>
     <td rowspan="2" valign="top">
        Recursively descends XML document piecing together
        the Text and CDATA nodes that are encountered.
        You can think of this as returning the plaintext
        version of the XML data, that is, with all tags removed.
    </td>
    </tr>
    <td>Element</td><td>kCFXMLNodeTypeElement</td>
   </tr>
   <tr>
    <td>Attribute</td><td>kCFXMLNodeTypeAttribute</td>
    <td rowspan="13" valign="top">
        Default action: Whatever is returned by Apple's
        <tt>CFXMLNodeGetString(...)</tt> method.
    </td>
   </tr>
    <tr><td>Processing Instruction</td><td>kCFXMLNodeTypeProcessingInstruction</td></tr>
    <tr><td>Comment</td><td>kCFXMLNodeTypeComment</td></tr>
    <tr><td>Text</td><td>kCFXMLNodeTypeText</td></tr>
    <tr><td>CDATA Section</td><td>kCFXMLNodeTypeCDATASection</td></tr>
    <tr><td>Document Fragment</td><td>kCFXMLNodeTypeDocumentFragment</td></tr>
    <tr><td>Entity</td><td>kCFXMLNodeTypeEntity</td></tr>
    <tr><td>Entity Reference</td><td>kCFXMLNodeTypeEntityReference</td></tr>
    <tr><td>Document Type</td><td>kCFXMLNodeTypeDocumentType</td></tr>
    <tr><td>Whitespace</td><td>kCFXMLNodeTypeWhitespace</td></tr>
    <tr><td>Notation Element</td><td>kCFXMLNodeTypeNotation</td></tr>
    <tr><td>Element Type Declaration</td><td>kCFXMLNodeTypeElementTypeDeclaration</td></tr>
    <tr><td>Attribute List Declaration</td><td>kCFXMLNodeTypeAttributeListDeclaration</td></tr>
   </tr>
  </tbody>
 </table>

 @result A textual representation of the <tt>XMLTree</tt>.
 */
-(NSString *)description;


/*!
 @method attributeNamed:
 @abstract Returns the attribute named <var>name</var>.
 @discussion
  Returns the attribute named <var>name</var> or
  <tt>nil</tt> if no such attribute is found or
  the node is not an Element node.
 @param name The name of the attribute to return.
 @result The attribute named <var>name</var>.
 */
-(NSString *)attributeNamed:(NSString *)name;



/*!
 @method attributes
 @abstract Returns a dictionary of all the attributes.
 @discussion
  Returns a dictionary of all the attributes in the node
  or <tt>nil</tt> if the node is not an Element node.
 @result A dictionary of all the attributes.
 */
-(NSDictionary *)attributes;


/*!
 @method type
 @abstract Returns the type of node this is.
 @discussion
  Returns the type of node this is as defined by Apple's
  <tt>enum</tt>:
  <pre>
 enum CFXMLNodeTypeCode {
     kCFXMLNodeTypeDocument = 1,
     kCFXMLNodeTypeElement = 2,
     kCFXMLNodeTypeAttribute = 3,
     kCFXMLNodeTypeProcessingInstruction = 4,
     kCFXMLNodeTypeComment = 5,
     kCFXMLNodeTypeText = 6,
     kCFXMLNodeTypeCDATASection = 7,
     kCFXMLNodeTypeDocumentFragment = 8,
     kCFXMLNodeTypeEntity = 9,
     kCFXMLNodeTypeEntityReference = 10,
     kCFXMLNodeTypeDocumentType = 11,
     kCFXMLNodeTypeWhitespace = 12,
     kCFXMLNodeTypeNotation = 13,
     kCFXMLNodeTypeElementTypeDeclaration = 14,
     kCFXMLNodeTypeAttributeListDeclaration = 15
 };
 </pre>
 @result The type of node this is.
 */
-(CFXMLNodeTypeCode)type;




/* ********  A B O U T   P A R E N T  ******** */



/*!
 @method parent
 @abstract Returns the parent of the tree.
 @discussion
  Returns the parent of the tree or <tt>nil</tt>
  if the parent does not exist.
 @result The tree's parent.
 */
-(XMLTree *)parent;



/* ********  A B O U T   C H I L D R E N  ******** */




    /*!
    @method xpath:
     @abstract Returns the value indicated by the xpath.
     @discussion
     Returns the value indicated by the xpath.
     Only basic xpath values are supported at this time.
     Basically it supports accessing elements and attributes.
     @param xpath The xpath to evaluate
     @result The XMLTree or NSString of the result.
     */
-(id)xpath:(NSString *)xpath;



/*!
 @method childAtIndex:
 @abstract Returns the child at the given index.
 @discussion
  Returns the child at the given index or <tt>nil</tt>
  if no such child exists or it doesn't make sense
  to have children (such as a Processing Instruction node).
 @param index The index of the child to get.
 @result The child at <var>index</var>.
 */
-(XMLTree *)childAtIndex:(int)index;




/*!
 @method childNamed:
 @abstract Returns the first child named <var>name</var>.
 @discussion
 Returns the first child named <var>name</var> or <tt>nil</tt>
 if no such child exists or it doesn't make sense
 to have children (such as a Processing Instruction node).
 @param name The name of the child.
 @result The child named <var>name</var>.
*/
-(XMLTree *)childNamed:(NSString *)name;


    /*!
    @method childNamed:withAttribute:equalTo:
     @abstract Returns the first child named <var>name</var>
      that has a matching attribute.
     @discussion
     Returns the first child named <var>name</var> with
     attribute <var>attrName</var> equal to <var>attrVal</var>
     or <tt>nil</tt> if no such child exists or it doesn't make sense
     to have children (such as a Processing Instruction node).
     @param name The name of the child.
     @param attrName The name of the attribute to look up.
     @param attrVal  The attribute value to match.
     @result The matching child.
     */
-(XMLTree *)childNamed:(NSString *)name
         withAttribute:(NSString *)attrName
               equalTo:(NSString *)attrVal;


/*!
 @method descendentNamed:
 @abstract Returns the first descendent named <var>name</var>.
 @discussion
  Returns the descendent named <var>name</var> or <tt>nil</tt>
  if no such descendent exists or it doesn't make sense
  to have descendents (such as a Processing Instruction node).
  This is a depth-first search.
 @param name The name of the child.
 @result The child named <var>name</var>.
*/
-(XMLTree *)descendentNamed:(NSString *)name;


/*!
 @method count
 @abstract Returns the number of children in the tree.
 @discussion
  Returns the number of children in the tree or <tt>-1</tt>
  if there is no valid tree contained within (like if you
  tried to create an <tt>XMLTree</tt> with <tt>init</tt>).
 @result The number of children in the tree.
 */
-(int)count;


@end

/*!
 @function XMLTreeDescription
 @abstract Used internally to recursively generate tree descriptions.
 @discussion
  Used internally to recursively generate tree descriptions.
  The returned string is just the mutable string that was passed in.
  It's retain count is unchanged.
 @param descr The mutable string that will have descriptions appended to it.
 @param tree The tree from which to make a description.
 @result A description of <var>tree</var>.
  It is the <var>descr</var> that was passed in.
 */
CFStringRef XMLTreeDescription( CFMutableStringRef descr, CFXMLTreeRef tree );



/*!
@function XMLTreeDescendentNamed
 @abstract Used internally to recursively search for a descendent.
 @discussion
  Used internally to recursively search for a descendent.
  The tree that is returned, if any, has a retain count of one more
  than it ought (it cannot be autoreleased, so far as I know), so
  you are responsible for releasing it, whether or not you retain it.
 @param name The name of the descendent to search for.
 @param tree The tree in which to recursively search.
 @result The matching descendent or <tt>NULL</tt> if no descendent is found.
 */
CFXMLTreeRef XMLTreeDescendentNamed( CFStringRef name, CFXMLTreeRef tree );




CFTypeRef XMLTreeXPath( CFMutableStringRef xpath, CFXMLTreeRef tree );
 
