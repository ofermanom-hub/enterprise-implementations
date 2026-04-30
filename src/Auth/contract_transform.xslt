<!-- Transforms enterprise contract XML into a
     signature-ready HTML document for DocuSign workflows -->
<xsl:stylesheet
    version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ct="urn:enterprise:contract:v1">

  <xsl:output method="html" indent="yes"
      encoding="UTF-8"/>

  <xsl:template match="/ct:Contract">
    <html><body style="font-family:Arial;max-width:720px">

      <h1>
        <xsl:value-of select="ct:Header/ct:Title"/>
      </h1>

      <table border="1" cellpadding="6">
        <tr>
          <th>Party</th><th>Role</th><th>Email</th>
        </tr>
        <xsl:for-each select="ct:Parties/ct:Party">
          <tr>
            <td><xsl:value-of select="ct:Name"/></td>
            <td><xsl:value-of select="ct:Role"/></td>
            <td><xsl:value-of select="ct:Email"/></td>
          </tr>
        </xsl:for-each>
      </table>

      <xsl:for-each select="ct:Clauses/ct:Clause">
        <h3>
          <xsl:value-of select="@number"/>.
          <xsl:value-of select="ct:Title"/>
        </h3>
        <p><xsl:value-of select="ct:Body"/></p>
      </xsl:for-each>

      <!-- Anchor tag picked up by DocuSign tab detection -->
      <p>Signature: <span
          style="color:white">/sig1/</span></p>

    </body></html>
  </xsl:template>

</xsl:stylesheet>
