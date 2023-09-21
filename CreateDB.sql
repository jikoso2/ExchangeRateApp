CREATE DATABASE "ExchangeRateDB"
    WITH
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'English_United States.1252'
    LC_CTYPE = 'English_United States.1252'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;


CREATE TABLE IF NOT EXISTS public.currency
(
    id integer NOT NULL DEFAULT nextval('currency_id_seq'::regclass),
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    code character varying(3) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT currency_pkey PRIMARY KEY (id),
    CONSTRAINT unique_code UNIQUE (code)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.currency
    OWNER to postgres;


CREATE TABLE IF NOT EXISTS public.rate
(
    ratedate date NOT NULL DEFAULT CURRENT_DATE,
    currency_id integer NOT NULL,
    value real NOT NULL,
    id integer NOT NULL DEFAULT nextval('rate_id_seq'::regclass),
    CONSTRAINT rate_pkey PRIMARY KEY (id),
    CONSTRAINT rate_currency_id_fkey FOREIGN KEY (currency_id)
        REFERENCES public.currency (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.rate
    OWNER to postgres;