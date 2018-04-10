# NHapiValidatator
NHapi validator for HL7 V2 messages.
NHapi does not throw any errors on hl7 v2 structure failures but it constructs the object.
This code helps to find out structural errors by using ExtraComponent property on IType.
