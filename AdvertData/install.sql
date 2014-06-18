-- Database: advertdata

-- DROP DATABASE advertdata;

CREATE DATABASE advertdata
  WITH OWNER = postgres
       ENCODING = 'UTF8'
       TABLESPACE = pg_default
       LC_COLLATE = 'Russian_Russia.1251'
       LC_CTYPE = 'Russian_Russia.1251'
       CONNECTION LIMIT = -1;

-- Table: advertdata

-- DROP TABLE advertdata;

CREATE TABLE advertdata
(
  advertdata_id serial NOT NULL,
  advertdata_starttime timestamp without time zone NOT NULL,
  advertdata_endtime timestamp without time zone NOT NULL,
  advertdata_key character varying(20) NOT NULL,
  advertdata_dll character varying(255) NOT NULL,
  advertdata_resource text NOT NULL,
  advertdata_url text NOT NULL,
  advertdata_user character varying(255) NOT NULL,
  advertdata_result character varying(255) NOT NULL,
  advertdata_ad character varying(255),
  CONSTRAINT advertdata_pkey PRIMARY KEY (advertdata_id)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE advertdata
  OWNER TO postgres;

-- Table: advertdata_settings

-- DROP TABLE advertdata_settings;

CREATE TABLE advertdata_settings
(
  advertdata_name character varying(255) NOT NULL,
  advertdata_value character varying(255) NOT NULL,
  CONSTRAINT advertdata_settings_pkey PRIMARY KEY (advertdata_name)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE advertdata_settings
  OWNER TO postgres;

-- View: advertdata_report

-- DROP VIEW advertdata_report;

CREATE OR REPLACE VIEW advertdata_report AS 
 SELECT advertdata.advertdata_id,
    advertdata.advertdata_starttime,
    advertdata.advertdata_endtime,
    advertdata.advertdata_key,
    advertdata.advertdata_dll,
    advertdata.advertdata_resource,
    advertdata.advertdata_url,
    advertdata.advertdata_user,
    advertdata.advertdata_result,
    advertdata.advertdata_ad,
    advertdata.advertdata_endtime - advertdata.advertdata_starttime AS advertdata_long
   FROM advertdata;

ALTER TABLE advertdata_report
  OWNER TO postgres;

-- View: advertdata_starttime

-- DROP VIEW advertdata_starttime;

CREATE OR REPLACE VIEW advertdata_starttime AS 
 SELECT DISTINCT advertdata.advertdata_starttime
   FROM advertdata;

ALTER TABLE advertdata_starttime
  OWNER TO postgres;

-- View: advertdata_endtime

-- DROP VIEW advertdata_endtime;

CREATE OR REPLACE VIEW advertdata_endtime AS 
 SELECT DISTINCT advertdata.advertdata_endtime
   FROM advertdata;

ALTER TABLE advertdata_endtime
  OWNER TO postgres;
