<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:template match="/SolutionCheckResult">
    <html>
      <body>
        <h1>Solution sanity check result</h1>
        <p><xsl:value-of select="@Path"/></p>
        <xsl:apply-templates/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="PackageVersionMismatches">
    <h2>Package version mismatches</h2>
    <xsl:if test=". = ''">
      <h3>No discrepancies found</h3>
    </xsl:if>
    <xsl:for-each select="PackageVersionMismatch">
      <h3>Package: <xsl:value-of select="@PackageName"/></h3>
      <xsl:for-each select="ProjectsReferencing">
        <h4>Projets referencing version: <b><xsl:value-of select="@PackageVersion"/></b></h4>
        <ul>
          <xsl:for-each select="Project">
            <li><xsl:value-of select="@Name"/></li>
          </xsl:for-each>
        </ul>
      </xsl:for-each>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="Projects">
    <h2>Solution projects (<xsl:value-of select="count(Project)"/>)</h2>
    <ul>
      <xsl:for-each select="Project">
        <li><b><xsl:value-of select="@Name"/></b> (<xsl:value-of select="@Path"/>)</li>
      </xsl:for-each>
    </ul>
  </xsl:template>

  <xsl:template match="NuGetPackages">
    <h2>Solution NuGet packages (<xsl:value-of select="count(NuGetPackage)"/>)</h2>
    <ul>
      <xsl:for-each select="NuGetPackage">
        <li><b><xsl:value-of select="@Id"/></b>&#xA0;&#xA0;&#xA0;<xsl:value-of select="@Version"/></li>
      </xsl:for-each>
    </ul>
  </xsl:template>

</xsl:stylesheet>