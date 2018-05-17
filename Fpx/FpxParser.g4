parser grammar FpxParser;

options { tokenVocab=FpxLexer; }

/*
 * Parser rules
 */
pxfile
    :   CHARSET EQUALS CHARSETVAL SEMICOL 
        (AXISVERSION EQUALS str SEMICOL)??
        CODEPAGE EQUALS CODEPAGEVAL SEMICOL
        LANGUAGE EQUALS LANGSLO SEMICOL
        languages
		creationdate
		(nextupdate)??
		(pxserver)??
		(directorypath)??
		(updatefrequency)??
		(tableid)??
		(synonym)??
		(DEFAULTGRAPH EQUALS INTVAL SEMICOL)??
		DECIMALS EQUALS INTVAL SEMICOL				// TODO: DECIMALS checking
		(SHOWDECIMALS EQUALS INTVAL SEMICOL)??
		(ROUNDING EQUALS INTVAL SEMICOL)??
        MATRIX EQUALS str SEMICOL
		(AGGREGALLOWED EQUALS yesno SEMICOL)??
		(AUTOPEN EQUALS yesno SEMICOL)??
        SUBJECTCODE EQUALS str SEMICOL
        subjectareas
		(CONFIDENTIAL EQUALS str SEMICOL)??
		COPYRIGHT EQUALS YES SEMICOL
		DESCRIPTION EQUALS linesofstring SEMICOL
		DESCRIPTION LANGDECLAR EQUALS linesofstring SEMICOL
		TITLE EQUALS linesofstring SEMICOL
		TITLE LANGDECLAR EQUALS linesofstring SEMICOL
		(DESCRIPTIONDEFAULT EQUALS yesno SEMICOL)??
		contents
		unitss
		stubs
		headings
		(contvariables)??
		pxvaluess
		(timevals)??
		codes
		(doublecolumns)??
		(PRESTEXT EQUALS INTVAL SEMICOL
		 PRESTEXT LANGDECLAR EQUALS INTVAL SEMICOL)??
		(domains)??
		(variabletypes)??
		(hierarchies)??
		(hierarchylevels)??
		(hierarchylevelsopen)??
		(hierarchynames)??
		(maps)??
		(partitioneds)??
		(eliminations)??
		(precisions)??
		lastupdated
		(stockfas)??
		(cfpricess)??
		(dayadjs)??
		(seasadjs)??
		contacts
		(refperiods)??
		(baseperiods)??
		DATABASE EQUALS linesofstring SEMICOL
		DATABASE LANGDECLAR EQUALS linesofstring SEMICOL
		SOURCE EQUALS linesofstring SEMICOL
		SOURCE LANGDECLAR EQUALS linesofstring SEMICOL
		(surveys)??
		(links)??
		(INFOFILE EQUALS linesofstring SEMICOL
		INFOFILE LANGDECLAR EQUALS linesofstring SEMICOL)??
		(firstpublished)??
		(metaids)??
		(OFFICIALSTATISTICS EQUALS yesno SEMICOL)??
		(INFO EQUALS linesofstring SEMICOL
		INFO LANGDECLAR EQUALS linesofstring SEMICOL)??
		notexs
		(notes)??
		(valuenotexs)??
		(valuenotes)??
		(cellnotexs)??
		(cellnotes)??
		DATASYMBOL1 EQUALS str SEMICOL
		DATASYMBOL1 LANGDECLAR EQUALS str SEMICOL
		DATASYMBOL2 EQUALS str SEMICOL
		DATASYMBOL2 LANGDECLAR EQUALS str SEMICOL
		DATASYMBOL3 EQUALS str SEMICOL
		DATASYMBOL3 LANGDECLAR EQUALS str SEMICOL
		DATASYMBOL4 EQUALS str SEMICOL
		DATASYMBOL4 LANGDECLAR EQUALS str SEMICOL
		(DATASYMBOL5 EQUALS str SEMICOL
		DATASYMBOL5 LANGDECLAR EQUALS str SEMICOL)??
		(DATASYMBOL6 EQUALS str SEMICOL
		DATASYMBOL6 LANGDECLAR EQUALS str SEMICOL)??
		(DATASYMBOLSUM EQUALS str SEMICOL
		DATASYMBOLSUM LANGDECLAR EQUALS str SEMICOL)??
		(DATASYMBOLNIL EQUALS str SEMICOL
		DATASYMBOLNIL LANGDECLAR EQUALS str SEMICOL)??
		(datanotecells)??
		(DATANOTESUM EQUALS str SEMICOL
		DATANOTESUM LANGDECLAR EQUALS str SEMICOL)??
		(datanotes)??
		(keyss)??
		(ATTRIBUTEID EQUALS stringlist SEMICOL)??
		(ATTRIBUTETEXT EQUALS stringlist SEMICOL
		ATTRIBUTETEXT LANGDECLAR EQUALS stringlist SEMICOL)??
		(attributess)??
		DATA
		//data
		EOF
    ;
yesno
	: YES
	| NO
	;
multistring
	: str
	| MULTILINESTRINGVAL
	;

stringlist
	: (str COMMA)+ str
	| str
	;
str
	: STRINGVAL
	| F
	| S
	| A
	| C
	;

multistringlist
	: (multistring COMMA)* multistring
	;

linesofstring
	: multistring+ 
	;

listlinesofstring
	: (linesofstring COMMA)* linesofstring
	;

languages
	: LANGUAGES EQUALS LANGSVAL SEMICOL
	;

creationdate
	: CREATIONDATE EQUALS DATETIMEVAL SEMICOL
	;

nextupdate
	: NEXTUPDATE EQUALS DATETIMEVAL SEMICOL
	;

pxserver
	: PXSERVER EQUALS multistring SEMICOL
	;

directorypath
	: DIRECTORYPATH EQUALS multistring SEMICOL
	;

updatefrequency
	: UPDATEFREQUENCY EQUALS multistring SEMICOL
	;

tableid
	: TABLEID EQUALS multistring SEMICOL
	;

synonym
	: SYNONYMS EQUALS multistring SEMICOL
	;

subjectareas
	: subjectarea+
	;
subjectarea
	: SUBJECTAREA LANGDECLAR?? EQUALS str SEMICOL
	;

contents
	: content+
	;
content
	: CONTENTS LANGDECLAR?? EQUALS str SEMICOL
	;

unitss
	: units+
	;
units
	: UNITS LANGDECLAR?? EQUALS str SEMICOL
	;

stubs
	: stub+
	;
stub
	: STUB LANGDECLAR?? EQUALS stringlist SEMICOL
	;	

headings
	: heading+
	;
heading
	: HEADING LANGDECLAR?? EQUALS stringlist SEMICOL
	;

contvariables
	: contvariable+
	;
contvariable
	: CONTVARIABLE LANGDECLAR?? EQUALS multistring SEMICOL
	;

pxvaluess
	: pxvalues+
	;
pxvalues
	: VALUES LANGDECLAR?? DIMDECLAR EQUALS multistringlist SEMICOL
	;

timevals
	: timeval+
	;
timeval
	: TIMEVAL LANGDECLAR?? DIMDECLAR EQUALS TLIST COMMA stringlist SEMICOL
	;

codes
	: code+
	;
code
	: CODES LANGDECLAR?? DIMDECLAR EQUALS stringlist SEMICOL
	;

doublecolumns
	: doublecolumn+
	;
doublecolumn
	: DOUBLECOLUMN LANGDECLAR?? DIMDECLAR EQUALS yesno SEMICOL		// biling
	;

domains
	: domain+
	;
domain
	: DOMAIN LANGDECLAR?? DIMDECLAR EQUALS multistring SEMICOL
	;

variabletypes
	: variabletype+
	;
variabletype 
	: VARIABLETYPE LANGDECLAR?? EQUALS multistring SEMICOL
	;

hierarchies
	: hierarchy+
	;
hierarchy
	: HIERARCHIES LANGDECLAR?? DIMDECLAR EQUALS hierar_vals SEMICOL
	;
hierar_vals
	: hierar_val COMMA hierar_vals
	| hierar_val
	;
hierar_val
	: str
	| str COLON str
	;

hierarchylevels
	: hierarchylevel+
	;
hierarchylevel
	: HIERARCHYLEVELS LANGDECLAR?? DIMDECLAR EQUALS INTVAL SEMICOL
	;

hierarchylevelsopen
	: hierarchylevelopen+
	;
hierarchylevelopen
	: HIERARCHYLEVELSOPEN LANGDECLAR?? DIMDECLAR EQUALS INTVAL SEMICOL
	;

hierarchynames
	: hierarchyname+
	;
hierarchyname
	: HIERARCHYNAMES LANGDECLAR?? DIMDECLAR EQUALS stringlist SEMICOL
	;

maps
	: map+
	;
map
	: MAP LANGDECLAR?? DIMDECLAR EQUALS stringlist SEMICOL
	;

partitioneds
	: partitioned+
	;
partitioned
	: PARTITIONED LANGDECLAR?? DIMDECLAR EQUALS complex_text SEMICOL
	;

complex_text
	: compl_val COMMA complex_text 
	| compl_val
	;
compl_val
	: STRINGVAL
	| F
	| S
	| A
	| C
	| INTVAL
	;

eliminations
	: elimination+
	;
elimination
	: ELIMINATION LANGDECLAR?? DIMDECLAR EQUALS elim_val SEMICOL
	;
elim_val
	: YES
	| NO
	| multistring
	;

precisions
	: precision+
	;
precision
	: PRECISION LANGDECLAR?? multidimdeclar EQUALS INTVAL SEMICOL
	;

multidimdeclar
	: LPARENTH stringlist RPARENTH
	| DIMDECLAR
	;

lastupdated
	: LASTUPDATED EQUALS DATETIMEVAL SEMICOL
	;

stockfas
	: stockfa+
	;
stockfa
	: STOCKFA LANGDECLAR?? DIMDECLAR?? EQUALS (F | S | A) SEMICOL
	;

cfpricess
	: cfprices+
	;
cfprices
	: CFPRICES LANGDECLAR?? DIMDECLAR?? EQUALS (C | F) SEMICOL
	;

dayadjs
	: dayadj+
	;
dayadj
	: DAYADJ LANGDECLAR?? DIMDECLAR?? EQUALS yesno SEMICOL
	;

seasadjs
	: seasadj+
	;
seasadj
	: SEASADJ LANGDECLAR?? DIMDECLAR?? EQUALS yesno SEMICOL
	;

contacts
	: contact+
	;
contact
	: CONTACT LANGDECLAR?? DIMDECLAR?? EQUALS linesofstring SEMICOL
	;

refperiods
	: refperiod+
	;
refperiod
	: REFPERIOD LANGDECLAR?? DIMDECLAR?? EQUALS linesofstring SEMICOL
	;

baseperiods
	: baseperiod+
	;
baseperiod
	: BASEPERIOD LANGDECLAR?? DIMDECLAR?? EQUALS linesofstring SEMICOL
	;

surveys
	: survey+
	;
survey
	: SURVEY LANGDECLAR?? EQUALS multistring SEMICOL
	;

links
	: link+
	;
link
	: LINK LANGDECLAR?? EQUALS multistring SEMICOL 
	;

firstpublished
	: FIRSTPUBLISHED EQUALS yesnodate SEMICOL
	;

yesnodate
	: yesno
	| DATETIMEVAL
	;

metaids
	: metaid+
	;
metaid
	: METAID LANGDECLAR?? DIMDECLAR?? EQUALS str SEMICOL
	;

notexs
	: notex+
	;
notex
	: NOTEX LANGDECLAR?? DIMDECLAR?? EQUALS linesofstring SEMICOL
	;

notes
	: note+
	;
note
	: NOTE LANGDECLAR?? DIMDECLAR?? EQUALS linesofstring SEMICOL
	;

valuenotexs
	: valuenotex+
	;
valuenotex
	: VALUENOTEX LANGDECLAR?? DIMDECLAR EQUALS linesofstring SEMICOL
	;

valuenotes
	: valuenote+
	;
valuenote
	: VALUENOTE LANGDECLAR?? DIMDECLAR EQUALS linesofstring SEMICOL
	;

cellnotexs
	: cellnotex+
	;
cellnotex
	: CELLNOTEX LANGDECLAR?? multidimdeclar EQUALS linesofstring SEMICOL
	;

cellnotes
	: cellnote+
	;
cellnote
	: CELLNOTE LANGDECLAR?? multidimdeclar EQUALS linesofstring SEMICOL
	;

datanotecells
	: datanotecell+
	;
datanotecell
	: DATANOTECELL LANGDECLAR?? multidimdeclar EQUALS str SEMICOL
	;

datanotes
	: datanote+
	;
datanote
	: DATANOTE LANGDECLAR?? multidimdeclar?? EQUALS multistring SEMICOL
	;

keyss
	: keys+
	;
keys
	: KEYS LANGDECLAR?? DIMDECLAR EQUALS keysval SEMICOL
	;
keysval
	: CODES 
	| VALUES
	;

attributess
	: attributes+
	;
attributes
	: ATTRIBUTES multidimdeclar?? EQUALS stringlist SEMICOL
	;

data
	: DATA EQUALS data_val_list SEMICOL 
	;
data_val_list
	: data_val+
	;
data_val
	: INTVAL
	| DECVAL
	| str
	;
