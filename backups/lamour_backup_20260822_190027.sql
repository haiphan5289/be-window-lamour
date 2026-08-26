--
-- PostgreSQL database dump
--

\restrict 926hdm9lo7X0dXRrQznYNb5xt8OpCQoPXtNRjRYCZtMFhAI0Bao2qedtSKWUaLq

-- Dumped from database version 16.14 (Homebrew)
-- Dumped by pg_dump version 16.14 (Homebrew)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public.warehouse_receipts DROP CONSTRAINT IF EXISTS "FK_warehouse_receipts_suppliers_supplier_id";
ALTER TABLE IF EXISTS ONLY public.warehouse_receipts DROP CONSTRAINT IF EXISTS "FK_warehouse_receipts_employees_employee_id";
ALTER TABLE IF EXISTS ONLY public.warehouse_receipts DROP CONSTRAINT IF EXISTS "FK_warehouse_receipts_customers_customer_id";
ALTER TABLE IF EXISTS ONLY public.warehouse_receipt_lines DROP CONSTRAINT IF EXISTS "FK_warehouse_receipt_lines_warehouses_warehouse_id";
ALTER TABLE IF EXISTS ONLY public.warehouse_receipt_lines DROP CONSTRAINT IF EXISTS "FK_warehouse_receipt_lines_warehouse_receipts_warehouse_receip~";
ALTER TABLE IF EXISTS ONLY public.warehouse_receipt_lines DROP CONSTRAINT IF EXISTS "FK_warehouse_receipt_lines_products_product_id";
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS "FK_sales_returns_employees_employee_id";
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS "FK_sales_returns_customers_customer_id";
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS "FK_sales_return_lines_warehouses_warehouse_id";
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS "FK_sales_return_lines_sales_returns_sales_return_id";
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS "FK_sales_return_lines_products_product_id";
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS "FK_sales_orders_employees_employee_id";
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS "FK_sales_orders_customers_customer_id";
ALTER TABLE IF EXISTS ONLY public.sales_order_lines DROP CONSTRAINT IF EXISTS "FK_sales_order_lines_warehouses_warehouse_id";
ALTER TABLE IF EXISTS ONLY public.sales_order_lines DROP CONSTRAINT IF EXISTS "FK_sales_order_lines_sales_orders_sales_order_id";
ALTER TABLE IF EXISTS ONLY public.sales_order_lines DROP CONSTRAINT IF EXISTS "FK_sales_order_lines_products_product_id";
ALTER TABLE IF EXISTS ONLY public.receipts DROP CONSTRAINT IF EXISTS "FK_receipts_employees_CollectorEmployeeId";
ALTER TABLE IF EXISTS ONLY public.receipts DROP CONSTRAINT IF EXISTS "FK_receipts_customers_CustomerId";
ALTER TABLE IF EXISTS ONLY public.receipt_entries DROP CONSTRAINT IF EXISTS "FK_receipt_entries_sales_orders_SalesOrderId";
ALTER TABLE IF EXISTS ONLY public.receipt_entries DROP CONSTRAINT IF EXISTS "FK_receipt_entries_receipts_ReceiptId";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_warehouses_default_warehouse_id";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_product_units_product_unit_id";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_categories_category_id";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_account_settings_stock_account_id";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_account_settings_revenue_account_id";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_account_settings_return_account_id";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_account_settings_price_reduction_account_id";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_account_settings_discount_account_id";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "FK_products_account_settings_cost_account_id";
ALTER TABLE IF EXISTS ONLY public.product_warehouse_stocks DROP CONSTRAINT IF EXISTS "FK_product_warehouse_stocks_warehouses_warehouse_id";
ALTER TABLE IF EXISTS ONLY public.product_warehouse_stocks DROP CONSTRAINT IF EXISTS "FK_product_warehouse_stocks_products_product_id";
ALTER TABLE IF EXISTS ONLY public.payments DROP CONSTRAINT IF EXISTS "FK_payments_suppliers_SupplierId";
ALTER TABLE IF EXISTS ONLY public.payments DROP CONSTRAINT IF EXISTS "FK_payments_employees_PaymentEmployeeId";
ALTER TABLE IF EXISTS ONLY public.payment_entries DROP CONSTRAINT IF EXISTS "FK_payment_entries_payments_PaymentId";
ALTER TABLE IF EXISTS ONLY public.payment_entries DROP CONSTRAINT IF EXISTS "FK_payment_entries_expense_categories_ExpenseCategoryId";
ALTER TABLE IF EXISTS ONLY public.payment_entries DROP CONSTRAINT IF EXISTS "FK_payment_entries_account_settings_DebitAccountSettingId";
ALTER TABLE IF EXISTS ONLY public.payment_entries DROP CONSTRAINT IF EXISTS "FK_payment_entries_account_settings_CreditAccountSettingId";
ALTER TABLE IF EXISTS ONLY public.expense_categories DROP CONSTRAINT IF EXISTS "FK_expense_categories_departments_department_id";
ALTER TABLE IF EXISTS ONLY public.deposits DROP CONSTRAINT IF EXISTS "FK_deposits_sales_orders_source_sales_order_id";
ALTER TABLE IF EXISTS ONLY public.deposits DROP CONSTRAINT IF EXISTS "FK_deposits_employees_employee_id";
ALTER TABLE IF EXISTS ONLY public.deposits DROP CONSTRAINT IF EXISTS "FK_deposits_customers_customer_id";
ALTER TABLE IF EXISTS ONLY public.deposit_deductions DROP CONSTRAINT IF EXISTS "FK_deposit_deductions_sales_orders_sales_order_id";
ALTER TABLE IF EXISTS ONLY public.deposit_deductions DROP CONSTRAINT IF EXISTS "FK_deposit_deductions_deposits_deposit_id";
ALTER TABLE IF EXISTS ONLY public.customers DROP CONSTRAINT IF EXISTS "FK_customers_employees_sale_care_employee_id";
DROP INDEX IF EXISTS public."IX_warehouses_code";
DROP INDEX IF EXISTS public."IX_warehouse_receipts_supplier_id";
DROP INDEX IF EXISTS public."IX_warehouse_receipts_status";
DROP INDEX IF EXISTS public."IX_warehouse_receipts_employee_id";
DROP INDEX IF EXISTS public."IX_warehouse_receipts_customer_id";
DROP INDEX IF EXISTS public."IX_warehouse_receipts_accounting_date";
DROP INDEX IF EXISTS public."IX_warehouse_receipt_lines_warehouse_receipt_id";
DROP INDEX IF EXISTS public."IX_warehouse_receipt_lines_warehouse_id";
DROP INDEX IF EXISTS public."IX_warehouse_receipt_lines_product_id";
DROP INDEX IF EXISTS public."IX_suppliers_code";
DROP INDEX IF EXISTS public."IX_sales_returns_employee_id";
DROP INDEX IF EXISTS public."IX_sales_returns_document_number";
DROP INDEX IF EXISTS public."IX_sales_returns_customer_id";
DROP INDEX IF EXISTS public."IX_sales_returns_accounting_date";
DROP INDEX IF EXISTS public."IX_sales_return_lines_warehouse_id";
DROP INDEX IF EXISTS public."IX_sales_return_lines_sales_return_id";
DROP INDEX IF EXISTS public."IX_sales_return_lines_product_id";
DROP INDEX IF EXISTS public."IX_sales_orders_employee_id";
DROP INDEX IF EXISTS public."IX_sales_orders_document_number";
DROP INDEX IF EXISTS public."IX_sales_orders_customer_id";
DROP INDEX IF EXISTS public."IX_sales_orders_accounting_date";
DROP INDEX IF EXISTS public."IX_sales_order_lines_warehouse_id";
DROP INDEX IF EXISTS public."IX_sales_order_lines_sales_order_id";
DROP INDEX IF EXISTS public."IX_sales_order_lines_product_id";
DROP INDEX IF EXISTS public."IX_receipts_CustomerId";
DROP INDEX IF EXISTS public."IX_receipts_CollectorEmployeeId";
DROP INDEX IF EXISTS public."IX_receipt_entries_SalesOrderId";
DROP INDEX IF EXISTS public."IX_receipt_entries_ReceiptId";
DROP INDEX IF EXISTS public."IX_products_stock_account_id";
DROP INDEX IF EXISTS public."IX_products_revenue_account_id";
DROP INDEX IF EXISTS public."IX_products_return_account_id";
DROP INDEX IF EXISTS public."IX_products_product_unit_id";
DROP INDEX IF EXISTS public."IX_products_price_reduction_account_id";
DROP INDEX IF EXISTS public."IX_products_discount_account_id";
DROP INDEX IF EXISTS public."IX_products_default_warehouse_id";
DROP INDEX IF EXISTS public."IX_products_cost_account_id";
DROP INDEX IF EXISTS public."IX_products_code";
DROP INDEX IF EXISTS public."IX_products_category_id";
DROP INDEX IF EXISTS public."IX_product_warehouse_stocks_warehouse_id";
DROP INDEX IF EXISTS public."IX_product_warehouse_stocks_product_id_warehouse_id";
DROP INDEX IF EXISTS public."IX_product_units_name";
DROP INDEX IF EXISTS public."IX_payments_SupplierId";
DROP INDEX IF EXISTS public."IX_payments_PaymentEmployeeId";
DROP INDEX IF EXISTS public."IX_payment_entries_PaymentId";
DROP INDEX IF EXISTS public."IX_payment_entries_ExpenseCategoryId";
DROP INDEX IF EXISTS public."IX_payment_entries_DebitAccountSettingId";
DROP INDEX IF EXISTS public."IX_payment_entries_CreditAccountSettingId";
DROP INDEX IF EXISTS public."IX_expense_categories_department_id";
DROP INDEX IF EXISTS public."IX_expense_categories_code";
DROP INDEX IF EXISTS public."IX_employees_code";
DROP INDEX IF EXISTS public."IX_deposits_source_sales_order_id";
DROP INDEX IF EXISTS public."IX_deposits_employee_id";
DROP INDEX IF EXISTS public."IX_deposits_document_number";
DROP INDEX IF EXISTS public."IX_deposits_customer_id";
DROP INDEX IF EXISTS public."IX_deposits_accounting_date";
DROP INDEX IF EXISTS public."IX_deposit_deductions_sales_order_id";
DROP INDEX IF EXISTS public."IX_deposit_deductions_document_number";
DROP INDEX IF EXISTS public."IX_deposit_deductions_deposit_id";
DROP INDEX IF EXISTS public."IX_deposit_deductions_accounting_date";
DROP INDEX IF EXISTS public."IX_departments_name";
DROP INDEX IF EXISTS public."IX_customers_sale_care_employee_id";
DROP INDEX IF EXISTS public."IX_customers_code";
DROP INDEX IF EXISTS public."IX_categories_name";
DROP INDEX IF EXISTS public."IX_cash_transactions_accounting_date";
DROP INDEX IF EXISTS public."IX_account_settings_code";
ALTER TABLE IF EXISTS ONLY public.warehouses DROP CONSTRAINT IF EXISTS "PK_warehouses";
ALTER TABLE IF EXISTS ONLY public.warehouse_receipts DROP CONSTRAINT IF EXISTS "PK_warehouse_receipts";
ALTER TABLE IF EXISTS ONLY public.warehouse_receipt_lines DROP CONSTRAINT IF EXISTS "PK_warehouse_receipt_lines";
ALTER TABLE IF EXISTS ONLY public.suppliers DROP CONSTRAINT IF EXISTS "PK_suppliers";
ALTER TABLE IF EXISTS ONLY public.sales_returns DROP CONSTRAINT IF EXISTS "PK_sales_returns";
ALTER TABLE IF EXISTS ONLY public.sales_return_lines DROP CONSTRAINT IF EXISTS "PK_sales_return_lines";
ALTER TABLE IF EXISTS ONLY public.sales_orders DROP CONSTRAINT IF EXISTS "PK_sales_orders";
ALTER TABLE IF EXISTS ONLY public.sales_order_lines DROP CONSTRAINT IF EXISTS "PK_sales_order_lines";
ALTER TABLE IF EXISTS ONLY public.receipts DROP CONSTRAINT IF EXISTS "PK_receipts";
ALTER TABLE IF EXISTS ONLY public.receipt_entries DROP CONSTRAINT IF EXISTS "PK_receipt_entries";
ALTER TABLE IF EXISTS ONLY public.products DROP CONSTRAINT IF EXISTS "PK_products";
ALTER TABLE IF EXISTS ONLY public.product_warehouse_stocks DROP CONSTRAINT IF EXISTS "PK_product_warehouse_stocks";
ALTER TABLE IF EXISTS ONLY public.product_units DROP CONSTRAINT IF EXISTS "PK_product_units";
ALTER TABLE IF EXISTS ONLY public.payments DROP CONSTRAINT IF EXISTS "PK_payments";
ALTER TABLE IF EXISTS ONLY public.payment_entries DROP CONSTRAINT IF EXISTS "PK_payment_entries";
ALTER TABLE IF EXISTS ONLY public.expense_categories DROP CONSTRAINT IF EXISTS "PK_expense_categories";
ALTER TABLE IF EXISTS ONLY public.employees DROP CONSTRAINT IF EXISTS "PK_employees";
ALTER TABLE IF EXISTS ONLY public.deposits DROP CONSTRAINT IF EXISTS "PK_deposits";
ALTER TABLE IF EXISTS ONLY public.deposit_deductions DROP CONSTRAINT IF EXISTS "PK_deposit_deductions";
ALTER TABLE IF EXISTS ONLY public.departments DROP CONSTRAINT IF EXISTS "PK_departments";
ALTER TABLE IF EXISTS ONLY public.customers DROP CONSTRAINT IF EXISTS "PK_customers";
ALTER TABLE IF EXISTS ONLY public.categories DROP CONSTRAINT IF EXISTS "PK_categories";
ALTER TABLE IF EXISTS ONLY public.cash_transactions DROP CONSTRAINT IF EXISTS "PK_cash_transactions";
ALTER TABLE IF EXISTS ONLY public.backup_schedule DROP CONSTRAINT IF EXISTS "PK_backup_schedule";
ALTER TABLE IF EXISTS ONLY public.account_settings DROP CONSTRAINT IF EXISTS "PK_account_settings";
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS "PK___EFMigrationsHistory";
DROP TABLE IF EXISTS public.warehouses;
DROP TABLE IF EXISTS public.warehouse_receipts;
DROP TABLE IF EXISTS public.warehouse_receipt_lines;
DROP TABLE IF EXISTS public.suppliers;
DROP TABLE IF EXISTS public.sales_returns;
DROP TABLE IF EXISTS public.sales_return_lines;
DROP TABLE IF EXISTS public.sales_orders;
DROP TABLE IF EXISTS public.sales_order_lines;
DROP TABLE IF EXISTS public.receipts;
DROP TABLE IF EXISTS public.receipt_entries;
DROP TABLE IF EXISTS public.products;
DROP TABLE IF EXISTS public.product_warehouse_stocks;
DROP TABLE IF EXISTS public.product_units;
DROP TABLE IF EXISTS public.payments;
DROP TABLE IF EXISTS public.payment_entries;
DROP TABLE IF EXISTS public.expense_categories;
DROP TABLE IF EXISTS public.employees;
DROP TABLE IF EXISTS public.deposits;
DROP TABLE IF EXISTS public.deposit_deductions;
DROP TABLE IF EXISTS public.departments;
DROP TABLE IF EXISTS public.customers;
DROP TABLE IF EXISTS public.categories;
DROP TABLE IF EXISTS public.cash_transactions;
DROP TABLE IF EXISTS public.backup_schedule;
DROP TABLE IF EXISTS public.account_settings;
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Name: account_settings; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.account_settings (
    id integer NOT NULL,
    code character varying(20) NOT NULL,
    description character varying(200) NOT NULL
);


--
-- Name: account_settings_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.account_settings ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.account_settings_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: backup_schedule; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.backup_schedule (
    id integer NOT NULL,
    is_enabled boolean NOT NULL,
    time_of_day time without time zone NOT NULL,
    retention_days integer NOT NULL,
    last_run_at timestamp with time zone,
    interval_days integer DEFAULT 0 NOT NULL,
    directory character varying(500) DEFAULT ''::character varying NOT NULL
);


--
-- Name: backup_schedule_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.backup_schedule ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.backup_schedule_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: cash_transactions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.cash_transactions (
    id integer NOT NULL,
    accounting_date timestamp with time zone NOT NULL,
    document_date timestamp with time zone NOT NULL,
    receipt_number character varying(20),
    payment_number character varying(20),
    description character varying(500) NOT NULL,
    account character varying(10) NOT NULL,
    counter_account character varying(10) NOT NULL,
    debit_amount numeric(18,2) NOT NULL,
    credit_amount numeric(18,2) NOT NULL,
    person_name character varying(200),
    created_at timestamp with time zone NOT NULL
);


--
-- Name: cash_transactions_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.cash_transactions ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.cash_transactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: categories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.categories (
    id integer NOT NULL,
    name character varying(100) NOT NULL
);


--
-- Name: categories_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.categories ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.categories_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: customers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.customers (
    id integer NOT NULL,
    code character varying(20) NOT NULL,
    name character varying(200) NOT NULL,
    address character varying(500) NOT NULL,
    province character varying(100) NOT NULL,
    customer_group character varying(100) NOT NULL,
    tax_code character varying(20) NOT NULL,
    phone character varying(20) NOT NULL,
    sale_care_employee_id integer
);


--
-- Name: customers_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.customers ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.customers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: departments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.departments (
    id integer NOT NULL,
    name character varying(100) NOT NULL
);


--
-- Name: departments_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.departments ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.departments_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: deposit_deductions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.deposit_deductions (
    id integer NOT NULL,
    document_number character varying(50) NOT NULL,
    deposit_id integer NOT NULL,
    sales_order_id integer NOT NULL,
    amount numeric(18,2) NOT NULL,
    accounting_date timestamp with time zone NOT NULL,
    document_date timestamp with time zone NOT NULL,
    description character varying(500),
    created_at timestamp with time zone NOT NULL
);


--
-- Name: deposit_deductions_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.deposit_deductions ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.deposit_deductions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: deposits; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.deposits (
    id integer NOT NULL,
    document_number character varying(50) NOT NULL,
    accounting_date timestamp with time zone NOT NULL,
    document_date timestamp with time zone NOT NULL,
    customer_id integer NOT NULL,
    employee_id integer,
    description character varying(500),
    reference character varying(200),
    amount numeric(18,2) NOT NULL,
    remaining_balance numeric(18,2) NOT NULL,
    status integer DEFAULT 0 NOT NULL,
    created_at timestamp with time zone NOT NULL,
    source_sales_order_id integer
);


--
-- Name: deposits_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.deposits ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.deposits_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: employees; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.employees (
    id integer NOT NULL,
    name character varying(200) NOT NULL,
    phone character varying(20) DEFAULT ''::character varying NOT NULL,
    role character varying(20) NOT NULL,
    password_hash character varying(500) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    unit character varying(30) NOT NULL,
    bank_account_number character varying(30),
    bank_name character varying(100),
    job_title character varying(30) NOT NULL,
    code character varying(10) DEFAULT ''::character varying NOT NULL,
    gender character varying(10) DEFAULT 'Nam'::character varying NOT NULL
);


--
-- Name: employees_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.employees ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.employees_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: expense_categories; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.expense_categories (
    id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(200) NOT NULL,
    department_id integer,
    description character varying(500)
);


--
-- Name: expense_categories_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.expense_categories ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.expense_categories_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: payment_entries; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.payment_entries (
    "Id" integer NOT NULL,
    "PaymentId" integer NOT NULL,
    "Description" character varying(500) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "SubjectCode" character varying(50),
    "SubjectName" character varying(200),
    "BankAccount" character varying(100),
    "ExpenseCategoryId" integer,
    "CreditAccountSettingId" integer DEFAULT 0 NOT NULL,
    "DebitAccountSettingId" integer DEFAULT 0 NOT NULL
);


--
-- Name: payment_entries_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.payment_entries ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."payment_entries_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: payments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.payments (
    "Id" integer NOT NULL,
    "SupplierId" integer NOT NULL,
    "PayeeName" character varying(200) NOT NULL,
    "Address" character varying(500),
    "PaymentReason" character varying(30) NOT NULL,
    "PaymentEmployeeId" integer,
    "Attachment" character varying(500),
    "Reference" character varying(200),
    "AccountingDate" timestamp with time zone NOT NULL,
    "DocumentDate" timestamp with time zone NOT NULL,
    "DocumentNumber" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "ConfirmedAt" timestamp with time zone,
    "ReasonDetail" character varying(500),
    "Status" character varying(20) DEFAULT ''::character varying NOT NULL
);


--
-- Name: payments_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.payments ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."payments_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: product_units; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.product_units (
    id integer NOT NULL,
    name character varying(50) NOT NULL
);


--
-- Name: product_units_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.product_units ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.product_units_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: product_warehouse_stocks; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.product_warehouse_stocks (
    id integer NOT NULL,
    product_id integer NOT NULL,
    warehouse_id integer NOT NULL,
    quantity integer DEFAULT 0 NOT NULL
);


--
-- Name: product_warehouse_stocks_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.product_warehouse_stocks ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.product_warehouse_stocks_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: products; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.products (
    id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(200) NOT NULL,
    unit character varying(50) NOT NULL,
    cost_price numeric(18,2) NOT NULL,
    selling_price numeric(18,2) NOT NULL,
    stock_quantity integer DEFAULT 0 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    excise_tax_group character varying(100),
    export_tax_rate numeric(18,2),
    import_tax_rate numeric(18,2),
    tax_reduction_type character varying(20),
    vat_rate character varying(20),
    category_id integer,
    cost_account_id integer,
    default_warehouse_id integer,
    description text,
    discount_account_id integer,
    is_promotional_good boolean DEFAULT false NOT NULL,
    latest_purchase_price numeric(18,2) DEFAULT 0.0 NOT NULL,
    min_stock_quantity integer DEFAULT 0 NOT NULL,
    nature character varying(20) DEFAULT 'VatTuHangHoa'::character varying NOT NULL,
    origin character varying(200),
    price_reduction_account_id integer,
    product_unit_id integer,
    purchase_description text,
    return_account_id integer,
    revenue_account_id integer,
    sale_description text,
    special_goods_type character varying(100),
    stock_account_id integer,
    trade_discount_rate numeric(9,2) DEFAULT 0.0 NOT NULL,
    warranty_period character varying(100),
    is_deposit_product boolean DEFAULT false NOT NULL
);


--
-- Name: products_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.products ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.products_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: receipt_entries; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.receipt_entries (
    "Id" integer NOT NULL,
    "ReceiptId" integer NOT NULL,
    "Description" character varying(500) NOT NULL,
    "DebitAccount" character varying(20) NOT NULL,
    "CreditAccount" character varying(20) NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "SubjectCode" character varying(50),
    "SubjectName" character varying(200),
    "BankAccount" character varying(100),
    "SalesOrderId" integer
);


--
-- Name: receipt_entries_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.receipt_entries ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."receipt_entries_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: receipts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.receipts (
    "Id" integer NOT NULL,
    "CustomerId" integer NOT NULL,
    "PayerName" character varying(200) NOT NULL,
    "Address" character varying(500),
    "PaymentReason" character varying(30) NOT NULL,
    "CollectorEmployeeId" integer,
    "Attachment" character varying(500),
    "Reference" character varying(200),
    "AccountingDate" timestamp with time zone NOT NULL,
    "DocumentDate" timestamp with time zone NOT NULL,
    "DocumentNumber" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


--
-- Name: receipts_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.receipts ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."receipts_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sales_order_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_order_lines (
    id integer NOT NULL,
    sales_order_id integer NOT NULL,
    product_id integer NOT NULL,
    product_code character varying(50) NOT NULL,
    product_name character varying(200) NOT NULL,
    is_promotion boolean NOT NULL,
    unit character varying(50) NOT NULL,
    quantity integer NOT NULL,
    unit_price numeric(18,2) NOT NULL,
    amount numeric(18,2) NOT NULL,
    receivable_account character varying(20) NOT NULL,
    revenue_account character varying(20) NOT NULL,
    discount_rate numeric(5,2) DEFAULT 0.0 NOT NULL,
    tax_amount numeric(18,2) DEFAULT 0.0 NOT NULL,
    tax_rate numeric(5,2) DEFAULT 0.0 NOT NULL,
    is_amount_manual boolean DEFAULT false NOT NULL,
    warehouse_id integer DEFAULT 0,
    is_deposit_product boolean DEFAULT false NOT NULL
);


--
-- Name: sales_order_lines_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.sales_order_lines ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."sales_order_lines_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sales_orders; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_orders (
    id integer NOT NULL,
    document_number character varying(50) NOT NULL,
    accounting_date timestamp with time zone NOT NULL,
    document_date timestamp with time zone NOT NULL,
    customer_id integer NOT NULL,
    employee_id integer,
    description character varying(500),
    reference character varying(200),
    payment_terms character varying(200),
    payment_due_days integer,
    payment_due_date timestamp with time zone,
    notes character varying(1000),
    delivery_method character varying(200),
    payment_method character varying(200),
    total_amount numeric(18,2) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    status integer DEFAULT 0 NOT NULL,
    grand_total numeric(18,2) DEFAULT 0.0 NOT NULL,
    total_tax_amount numeric(18,2) DEFAULT 0.0 NOT NULL,
    customer_name_override character varying(200),
    customer_address_override character varying(500)
);


--
-- Name: sales_orders_Id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.sales_orders ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."sales_orders_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sales_return_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_return_lines (
    id integer NOT NULL,
    sales_return_id integer NOT NULL,
    product_id integer NOT NULL,
    product_code character varying(50) NOT NULL,
    product_name character varying(200) NOT NULL,
    return_account character varying(20) NOT NULL,
    debt_account character varying(20) NOT NULL,
    discount_account character varying(20) NOT NULL,
    unit character varying(50) NOT NULL,
    quantity integer NOT NULL,
    unit_price numeric(18,2) NOT NULL,
    amount numeric(18,2) NOT NULL,
    discount_rate numeric(5,2) DEFAULT 0.0 NOT NULL,
    discount_amount numeric(18,2) NOT NULL,
    sales_order_number character varying(50),
    warehouse_id integer DEFAULT 0 NOT NULL
);


--
-- Name: sales_return_lines_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.sales_return_lines ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.sales_return_lines_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sales_returns; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sales_returns (
    id integer NOT NULL,
    document_number character varying(50) NOT NULL,
    accounting_date timestamp with time zone NOT NULL,
    document_date timestamp with time zone NOT NULL,
    customer_id integer NOT NULL,
    employee_id integer,
    description character varying(500),
    reference character varying(200),
    return_type integer DEFAULT 0 NOT NULL,
    total_amount numeric(18,2) NOT NULL,
    total_discount numeric(18,2) NOT NULL,
    total_payment numeric(18,2) NOT NULL,
    created_at timestamp with time zone NOT NULL
);


--
-- Name: sales_returns_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.sales_returns ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.sales_returns_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: suppliers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.suppliers (
    id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(200) NOT NULL,
    address character varying(500) NOT NULL,
    "group" character varying(100) NOT NULL,
    tax_code character varying(20) NOT NULL,
    phone character varying(20) NOT NULL,
    is_stop_tracking boolean DEFAULT false NOT NULL
);


--
-- Name: suppliers_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.suppliers ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.suppliers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: warehouse_receipt_lines; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.warehouse_receipt_lines (
    id integer NOT NULL,
    warehouse_receipt_id integer NOT NULL,
    product_id integer NOT NULL,
    warehouse_id integer NOT NULL,
    quantity integer NOT NULL,
    unit_price numeric(18,2) NOT NULL,
    amount numeric(18,2) NOT NULL,
    debit_account character varying(20) NOT NULL,
    credit_account character varying(20) NOT NULL,
    cost_item character varying(100),
    cost_object character varying(100),
    loan_contract_number character varying(100),
    project character varying(100),
    purchase_order_number character varying(100),
    sales_contract_number character varying(100),
    statistics_code character varying(100)
);


--
-- Name: warehouse_receipt_lines_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.warehouse_receipt_lines ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.warehouse_receipt_lines_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: warehouse_receipts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.warehouse_receipts (
    id integer NOT NULL,
    receipt_number character varying(25) NOT NULL,
    receipt_type integer NOT NULL,
    status integer NOT NULL,
    customer_id integer,
    employee_id integer,
    accounting_date timestamp with time zone NOT NULL,
    document_date timestamp with time zone NOT NULL,
    description character varying(300),
    delivery_person character varying(200),
    reference character varying(100),
    total_amount numeric(18,2) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    confirmed_at timestamp with time zone,
    supplier_id integer
);


--
-- Name: warehouse_receipts_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.warehouse_receipts ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.warehouse_receipts_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: warehouses; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.warehouses (
    id integer NOT NULL,
    code character varying(20) NOT NULL,
    name character varying(100) NOT NULL,
    is_active boolean DEFAULT true NOT NULL
);


--
-- Name: warehouses_id_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.warehouses ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public.warehouses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260425035040_InitialCreate	9.0.3
20260425045914_ProductsCreate	9.0.3
20260425052915_CustomersCreate	9.0.3
20260425083129_EmployeeCreate	9.0.3
20260426020155_AddEmployeeUnit	9.0.3
20260426020606_ChangeEmployeeUnitDefaultToSpa	9.0.3
20260426025156_AddEmployeeJobTitleAndBankInfo	9.0.3
20260426075209_AddCashTransactions	9.0.3
20260426082013_AddPaymentReceipts	9.0.3
20260426085311_AddEmployeeCode	9.0.3
20260426093512_AddPaymentReceiptDescription	9.0.3
20260427043847_AddWarehouseReceipts	9.0.3
20260429031657_RebuildReceipts	9.0.3
20260429100048_AddPayments	9.0.3
20260501034415_AddSalesOrders	9.0.3
20260501040054_AddDiscountRateToSalesOrderLines	9.0.3
20260523100425_AddSaleCareToCustomers	9.0.3
20260523102715_RenameSalesOrderColumnsToSnakeCase	9.0.3
20260523112710_AddProductTaxFields	9.0.3
20260611144825_SalesOrderStatus	9.0.3
20260613041536_SalesReturnCreate	9.0.3
20260715141956_SaleCareEmployeeIdAndSalesOrderTax	9.0.3
20260725093244_CategoriesCreate	9.0.3
20260726041323_BackupScheduleCreate	9.0.3
20260726044021_BackupScheduleAddIntervalDays	9.0.3
20260726050714_BackupScheduleAddDirectory	9.0.3
20260804141438_AddIsAmountManualToSalesOrderLines	9.0.3
20260809045433_AddDeposits	9.0.3
20260809102942_AddProductUnitsAndAccountSettings	9.0.3
20260809110425_ExtendProductForVTHHForm	9.0.3
20260809113714_AddDiscountReturnAccountSettings	9.0.3
20260810025045_AddProductWarehouseStock	9.0.3
20260810082526_AddDepartmentsAndExpenseCategories	9.0.3
20260810094907_AddPaymentStatusAndExpenseCategoryLink	9.0.3
20260810125950_ConvertPaymentAccountsToAccountSettingFk	9.0.3
20260815024707_UpdateWarehouseReceiptSupplierAndStats	9.0.3
20260815030926_CascadeDeleteProductWarehouseStock	9.0.3
20260815032908_MakeProductCategoryOptional	9.0.3
20260815035101_AddProductDepositLinking	9.0.3
20260818131059_ImportVatTuHangHoaAndTonKhoData	9.0.3
20260819082557_MakeSalesOrderLineWarehouseOptional	9.0.3
20260819130505_UpdateEmployeeGenderAndUnit	9.0.3
20260819132143_AddIsDepositProductToSalesOrderLines	9.0.3
20260821065930_AddSalesOrderIdToReceiptEntries	9.0.3
20260822041555_AddCustomerNameOverrideToSalesOrder	9.0.3
20260822043321_AddCustomerAddressOverrideToSalesOrder	9.0.3
\.


--
-- Data for Name: account_settings; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.account_settings (id, code, description) FROM stdin;
1	151	Hàng mua đang đi đường
2	152	Nguyên liệu, vật liệu
3	1531	Công cụ, dụng cụ
4	1532	Bao bì luân chuyển
5	1533	Đồ dùng cho thuê
6	1534	Thiết bị, phụ tùng thay thế
7	1551	Thành phẩm nhập kho
8	1557	Thành phẩm bất động sản
9	1561	Giá mua hàng hóa
10	1562	Chi phí thu mua hàng hóa
11	1567	Hàng hóa bất động sản
12	157	Hàng gửi đi bán
13	158	Hàng hóa kho bảo thuế
14	3339	Phí, lệ phí và các khoản phải nộp khác
15	5111	Doanh thu bán hàng hóa
16	5112	Doanh thu bán các thành phẩm
17	5113	Doanh thu cung cấp dịch vụ
18	5114	Doanh thu trợ cấp, trợ giá
19	5117	Doanh thu kinh doanh bất động sản đầu tư
20	5118	Doanh thu khác
21	711	Thu nhập khác
22	154	Chi phí sản xuất, kinh doanh dở dang
23	2411	Mua sắm TSCĐ
24	2412	Xây dựng cơ bản
25	2413	Sửa chữa lớn TSCĐ
26	242	Chi phí trả trước
27	6111	Mua nguyên liệu, vật liệu
28	6112	Mua hàng hóa
29	632	Giá vốn hàng bán
30	6232	Chi phí vật liệu
31	6412	Chi phí vật liệu, bao bì
32	6413	Chi phí dụng cụ, đồ dùng
33	6417	Chi phí dịch vụ mua ngoài
34	6422	Chi phí vật liệu quản lý
35	6423	Chi phí đồ dùng văn phòng
36	811	Chi phí khác
37	5211	Chiết khấu thương mại
38	5212	Hàng bán bị trả lại
39	5213	Giảm giá hàng bán
40	111	Tiền mặt
41	112	Tiền gửi ngân hàng
42	131	Phải thu của khách hàng
43	334	Phải trả người lao động
\.


--
-- Data for Name: backup_schedule; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.backup_schedule (id, is_enabled, time_of_day, retention_days, last_run_at, interval_days, directory) FROM stdin;
1	t	02:00:00	30	2026-08-19 02:00:33.130361+07	1	/Users/haiphan/Desktop/haiphan/be-window-lamour/backups
\.


--
-- Data for Name: cash_transactions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.cash_transactions (id, accounting_date, document_date, receipt_number, payment_number, description, account, counter_account, debit_amount, credit_amount, person_name, created_at) FROM stdin;
1	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	PT00678	\N	Thu tiền khách hàng	111	131	520000.00	0.00	Thanh Đức	2023-11-02 07:00:00+07
2	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	PT00678	\N	Thu tiền khách hàng	111	131	5850000.00	0.00	Thanh Đức	2023-11-02 07:00:00+07
3	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	PT00678	\N	Thu tiền khách hàng	111	131	5025000.00	0.00	Thanh Đức	2023-11-02 07:00:00+07
4	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	PT00678	\N	Thu tiền khách hàng	111	131	5640000.00	0.00	Thanh Đức	2023-11-02 07:00:00+07
5	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	PT00678	\N	Thu tiền khách hàng	111	131	5200000.00	0.00	Thanh Đức	2023-11-02 07:00:00+07
6	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	\N	PC02215	Diễm	111	6418	0.00	615000.00	LÊ HOÀNG THANH ĐỨC	2023-11-02 07:00:00+07
7	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	\N	PC02215	Thảo Uyên	111	6418	0.00	1055000.00	LÊ HOÀNG THANH ĐỨC	2023-11-02 07:00:00+07
8	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	\N	PC02215	Phúc Nhi	111	6418	0.00	1228000.00	LÊ HOÀNG THANH ĐỨC	2023-11-02 07:00:00+07
9	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	\N	PC02215	Hân	111	6418	0.00	174000.00	LÊ HOÀNG THANH ĐỨC	2023-11-02 07:00:00+07
10	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	\N	PC02215	Hương Ly	111	6418	0.00	105000.00	LÊ HOÀNG THANH ĐỨC	2023-11-02 07:00:00+07
11	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	\N	PC02216	Mua like fanpage tháng 10/2023	111	6418	0.00	450000.00	NGUYỄN HÀ THANH HÀ	2023-11-02 07:00:00+07
12	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	\N	PC02217	Phí lưu kho t10/2023	111	6418	0.00	1715000.00	LÊ HOÀNG THANH ĐỨC	2023-11-02 07:00:00+07
13	2023-11-02 07:00:00+07	2023-11-02 07:00:00+07	\N	PC02218	Thuê VP t11/2023	111	6418	0.00	40000000.00	LÊ HOÀNG THANH ĐỨC	2023-11-02 07:00:00+07
14	2026-08-11 07:00:00+07	2026-08-11 07:00:00+07	\N	PC00001	Công ty TNHH Mỹ phẩm Việt Hàn	111	1562	0.00	0.00	Công ty TNHH Mỹ phẩm Việt Hàn	2026-08-11 19:37:01.655172+07
15	2026-08-11 07:00:00+07	2026-08-11 07:00:00+07	\N	PC00002	Nhà phân phối Dược mỹ phẩm An Khang	111	1562	0.00	0.00	Nhà phân phối Dược mỹ phẩm An Khang	2026-08-11 19:37:41.887638+07
16	2026-08-11 07:00:00+07	2026-08-11 07:00:00+07	\N	PC00003	Nhà phân phối Dược mỹ phẩm An Khang	111	1562	0.00	10000000.00	Nhà phân phối Dược mỹ phẩm An Khang	2026-08-11 19:59:05.330315+07
\.


--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.categories (id, name) FROM stdin;
1	Chăm sóc da
2	Khuyến mại
3	Mặt nạ
4	sss
5	Vệ sinh da
\.


--
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.customers (id, code, name, address, province, customer_group, tax_code, phone, sale_care_employee_id) FROM stdin;
1	KH00001	PHƯƠNG HOA SPA	184 Bình Lợi, P.13, Bình Thạnh, TP.HCM	TP HỒ CHÍ MINH	Spa & Beauty		0937024285	2
2	KH00002	CHI NHI COSMETICS	351 Nguyễn Thiện Thuật, P.6, Q.3, TP.HCM	TP HỒ CHÍ MINH	Retail	0312345678	0932737477	3
3	KH00003	NGỌC ANH SALON	22 Lê Văn Sỹ, P.13, Q.3, TP.HCM	TP HỒ CHÍ MINH	Salon		0908111222	\N
4	KH00004	HOÀNG GIA SPA & CLINIC	58 Nguyễn Trãi, P.Bến Thành, Q.1, TP.HCM	TP HỒ CHÍ MINH	Spa & Beauty	0398765432	0918222333	2
5	KH00005	MAI LINH BEAUTY CARE	120 Cách Mạng Tháng 8, P.10, Q.3, TP.HCM	TP HỒ CHÍ MINH	Retail		0909333444	\N
6	KH00006	THU HƯƠNG SPA	15 Hai Bà Trưng, P.Bến Nghé, Q.1, TP.HCM	TP HỒ CHÍ MINH	Spa & Beauty		0938444555	\N
7	KH00007	Chi Nhi	123				0901234567	\N
\.


--
-- Data for Name: departments; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.departments (id, name) FROM stdin;
1	PHÒNG SALES
2	PHÒNG MARKETING
3	PHÒNG KHO VẬN
4	PHÒNG TÀI CHÍNH - KẾ TOÁN
5	PHÒNG NHÂN SỰ
6	PHÒNG ĐÀO TẠO
7	PHÒNG SPA
8	KHÁC
\.


--
-- Data for Name: deposit_deductions; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.deposit_deductions (id, document_number, deposit_id, sales_order_id, amount, accounting_date, document_date, description, created_at) FROM stdin;
1	TC00001	2	14	10000000.00	2026-08-14 07:00:00+07	2026-08-14 07:00:00+07	Trừ cọc thanh toán đơn BC00014	2026-08-15 11:08:11.913384+07
2	TC00002	4	24	2000000.00	2026-08-21 07:00:00+07	2026-08-21 07:00:00+07	Trừ cọc thanh toán đơn BH00001	2026-08-22 10:39:11.519043+07
3	TC00003	3	25	2000000.00	2026-08-21 07:00:00+07	2026-08-21 07:00:00+07	Trừ cọc thanh toán đơn BH00002	2026-08-22 10:45:48.356112+07
\.


--
-- Data for Name: deposits; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.deposits (id, document_number, accounting_date, document_date, customer_id, employee_id, description, reference, amount, remaining_balance, status, created_at, source_sales_order_id) FROM stdin;
1	DC00001	2026-08-07 07:00:00+07	2026-08-07 07:00:00+07	1	1	hihihahha	\N	2000000.00	2000000.00	0	2026-08-09 13:51:13.33101+07	\N
2	DC00002	2026-08-14 07:00:00+07	2026-08-14 07:00:00+07	1	2	Đặt cọc từ đơn BC00013	\N	20000000.00	10000000.00	0	2026-08-15 11:06:58.147157+07	13
4	DC00004	2026-08-19 07:00:00+07	2026-08-19 07:00:00+07	1	2	Đặt cọc từ đơn XK00016	\N	2000000.00	0.00	1	2026-08-19 20:28:28.498312+07	16
3	DC00003	2026-08-19 07:00:00+07	2026-08-19 07:00:00+07	1	2	Đặt cọc từ đơn XK00015	\N	2000000.00	0.00	1	2026-08-19 20:16:44.130111+07	15
\.


--
-- Data for Name: employees; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.employees (id, name, phone, role, password_hash, is_active, unit, bank_account_number, bank_name, job_title, code, gender) FROM stdin;
2	Nguyễn Văn An	0912345001	Cashier	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	t	Phòng Kinh Doanh	0071123456789	Vietcombank	NhanVienBanHang	NV00002	Nam
1	Admin	0901234567	Admin	6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=	t	Tiệm spa	\N	\N	Admin	NV00001	Nam
3	Trần Thị Bích	0912345002	Cashier	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	t	Tiệm spa	19012345678	Techcombank	ThuNgan	NV00003	Nam
5	Phạm Thị Dung	0912345004	Cashier	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	t	Phòng Giám Đốc	\N	\N	TruongPhong	NV00005	Nam
4	Lê Văn Cường	0912345003	Warehouse	jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=	t	Kho và Quỹ	\N	\N	NhanVienKho	NV00004	Nam
\.


--
-- Data for Name: expense_categories; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.expense_categories (id, code, name, department_id, description) FROM stdin;
1	111	sale	6	zzzz
\.


--
-- Data for Name: payment_entries; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.payment_entries ("Id", "PaymentId", "Description", "Amount", "SubjectCode", "SubjectName", "BankAccount", "ExpenseCategoryId", "CreditAccountSettingId", "DebitAccountSettingId") FROM stdin;
1	1	thuee xe 	0.00	\N	\N	\N	1	8	10
2	2	zzz	0.00	\N	\N	\N	1	8	10
3	3	qqqq	10000000.00	\N	\N	\N	1	8	10
4	4	tesst treo	5000000.00	\N	\N	\N	1	9	9
5	6	sssss	1000000.00	\N	\N	\N	1	10	10
\.


--
-- Data for Name: payments; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.payments ("Id", "SupplierId", "PayeeName", "Address", "PaymentReason", "PaymentEmployeeId", "Attachment", "Reference", "AccountingDate", "DocumentDate", "DocumentNumber", "CreatedAt", "ConfirmedAt", "ReasonDetail", "Status") FROM stdin;
1	1	Công ty TNHH Mỹ phẩm Việt Hàn	12 Trường Sơn, P.15, Q.10, TP.HCM	ChiKhac	\N	\N	\N	2026-08-11 07:00:00+07	2026-08-11 07:00:00+07	PC00001	2026-08-11 19:37:01.482029+07	2026-08-11 19:37:01.665869+07	\N	Confirmed
2	3	Nhà phân phối Dược mỹ phẩm An Khang	45 Lý Thường Kiệt, Q.10, TP.HCM	ChiKhac	\N	\N	\N	2026-08-11 07:00:00+07	2026-08-11 07:00:00+07	PC00002	2026-08-11 19:37:41.856871+07	2026-08-11 19:37:41.889463+07	\N	Confirmed
3	3	Nhà phân phối Dược mỹ phẩm An Khang	45 Lý Thường Kiệt, Q.10, TP.HCM	ChiKhac	\N	\N	\N	2026-08-11 07:00:00+07	2026-08-11 07:00:00+07	PC00003	2026-08-11 19:59:05.165112+07	2026-08-11 19:59:05.341128+07	\N	Confirmed
4	1	Công ty TNHH Mỹ phẩm Việt Hàn	12 Trường Sơn, P.15, Q.10, TP.HCM	ChiKhac	\N	\N	\N	2026-08-10 07:00:00+07	2026-08-10 07:00:00+07	PC00004	2026-08-11 20:01:26.946335+07	\N	\N	Treo
5	1	Công ty TNHH Mỹ phẩm Việt Hàn	12 Trường Sơn, P.15, Q.10, TP.HCM	ChiKhac	\N	\N	\N	2026-08-11 07:00:00+07	2026-08-11 07:00:00+07	PC00005	2026-08-11 20:30:29.177568+07	\N	\N	Treo
6	3	Nhà phân phối Dược mỹ phẩm An Khang	45 Lý Thường Kiệt, Q.10, TP.HCM	ChiKhac	\N	\N	\N	2026-08-11 07:00:00+07	2026-08-11 07:00:00+07	PC00006	2026-08-11 20:31:19.453806+07	\N	\N	Treo
\.


--
-- Data for Name: product_units; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.product_units (id, name) FROM stdin;
1	Cái
2	Hộp
3	Chai
4	Tuýp
5	Cuốn
6	Bộ
7	Set
8	Thùng
9	Gói
10	Lọ
\.


--
-- Data for Name: product_warehouse_stocks; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.product_warehouse_stocks (id, product_id, warehouse_id, quantity) FROM stdin;
1	6	4	78
6	1	4	43
7	2	4	28
8	8	4	36
9	7	4	998
10	6	1	1
3	4	4	93
11	11	5	3
12	11	4	216
14	13	4	107
15	14	4	221
16	15	4	207
17	16	4	13
18	17	5	3
19	17	4	193
20	18	5	3
21	18	4	163
22	20	4	106
23	21	5	10
24	21	4	318
25	22	4	120
26	23	5	3
27	23	4	24
28	24	5	3
30	25	5	3
31	25	4	30
32	26	5	5
33	26	4	152
34	27	5	3
35	27	4	1270
36	28	5	3
37	28	4	256
38	29	5	3
39	29	4	1436
40	30	5	3
41	30	4	151
42	31	5	3
43	31	4	1198
44	32	4	2878
45	33	4	143
46	34	5	3
47	34	4	65
48	35	5	3
49	35	4	400
50	36	5	3
51	36	4	227
52	37	5	3
53	37	4	23
54	38	5	3
55	38	4	50
56	39	5	3
57	39	4	80
58	41	5	10
59	41	4	357
60	42	5	3
61	42	4	69
62	43	5	3
63	43	4	127
64	44	4	1
65	45	5	3
66	45	4	73
67	46	5	3
68	46	4	256
69	47	5	3
70	47	4	93
71	48	5	3
72	48	4	1375
73	49	5	3
74	49	4	664
75	50	5	3
76	50	4	172
77	51	4	103
78	52	4	172
79	53	4	315
80	54	4	211
81	55	4	35
82	56	4	120
83	57	4	55
84	58	4	163
85	59	4	54
86	60	4	93
87	61	4	106
88	62	5	3
89	62	4	160
90	63	4	246
91	64	4	62
92	65	4	65
93	66	4	135
94	67	4	182
95	68	4	139
96	69	4	101
97	70	4	155
98	71	4	99
99	73	4	145
100	74	4	188
101	75	4	519
102	76	4	280
103	77	4	398
104	78	4	71
105	79	4	106
13	12	4	0
29	24	4	12
5	5	4	169
4	3	4	35
\.


--
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.products (id, code, name, unit, cost_price, selling_price, stock_quantity, is_active, excise_tax_group, export_tax_rate, import_tax_rate, tax_reduction_type, vat_rate, category_id, cost_account_id, default_warehouse_id, description, discount_account_id, is_promotional_good, latest_purchase_price, min_stock_quantity, nature, origin, price_reduction_account_id, product_unit_id, purchase_description, return_account_id, revenue_account_id, sale_description, special_goods_type, stock_account_id, trade_discount_rate, warranty_period, is_deposit_product) FROM stdin;
12	10	Meso Calming Ampoule Mask II 10M	Hộp	0.00	0.00	0	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
1	SP001	Skin Hydration Gel Toner	Chai	500000.00	1200000.00	43	t	\N	\N	\N	ChuaGiamThue	Eight	1	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
24	21	Soothing Massage Cream	Chai	0.00	0.00	15	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
5	SP005	Mặt nạ dưỡng ẩm	Miếng	15000.00	45000.00	169	t	\N	\N	\N	ChuaGiamThue	Ten	3	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
3	SP003	Centella TC Cream	Hộp	280000.00	650000.00	35	t	\N	\N	\N	ChuaGiamThue	Eight	1	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
2	SP002	Time Reset Serum	Chai	900000.00	2160000.00	28	t	\N	\N	\N	ChuaGiamThue	Eight	1	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
8	SP008	Tinh chất Vitamin C	Chai	650000.00	1450000.00	36	t	\N	\N	\N	\N	\N	1	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
7	SP007	Combo quà tặng dùng thử	Bộ	0.00	0.00	998	t	\N	\N	\N	ChuaGiamThue	Zero	2	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
6	SP006	Kem chống nắng SPF50	Tuýp	180000.00	420000.00	79	t	\N	\N	\N	ChuaGiamThue	Eight	1	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
10	DATCOC	Đặt cọc	Lần	0.00	0.00	0	t	\N	\N	\N	\N	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	t
4	SP004	Sữa rửa mặt Cocoon	Chai	120000.00	280000.00	93	t	\N	\N	\N	CoGiamThue	Five	5	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
11	1	Biocell Face Scrub	Chai	0.00	0.00	219	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
13	11	Mesotox Skin Booster V-Plot	Hộp	0.00	0.00	107	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
14	12	EXO LLT Pro TRI Fills Solution	Hộp	0.00	0.00	221	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
15	13	EXO LLT Pro AC Fills Solution	Hộp	0.00	0.00	207	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
16	14	Meso Filler	Cái	0.00	0.00	13	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	1	\N	\N	\N	\N	\N	\N	0.00	\N	f
17	15	Meso Fills	Hộp	0.00	0.00	196	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
18	16	Meso Hydro Ampoule Mask	Hộp	0.00	0.00	166	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
19	17	Meso Hydro Mask	Hộp	0.00	0.00	0	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
20	18	Numa Cream	Tuýp	0.00	0.00	106	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	4	\N	\N	\N	\N	\N	\N	0.00	\N	f
21	19	Ống Xillanh	Cái	0.00	0.00	328	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	1	\N	\N	\N	\N	\N	\N	0.00	\N	f
22	2	Antioxidant Cream Mask	Hộp	0.00	0.00	120	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
23	20	PH Balance Cleansing Lotion	Chai	0.00	0.00	27	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
25	22	PH Balance Toning Lotion	Chai	0.00	0.00	33	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
26	23	Skin Cooler	Hộp	0.00	0.00	157	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
27	24	UV Block	Hộp	0.00	0.00	1273	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
28	25	BB Cream	Tuýp	0.00	0.00	259	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	4	\N	\N	\N	\N	\N	\N	0.00	\N	f
29	26	Ves Serum	Hộp	0.00	0.00	1439	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
30	27	Bubble Cleanser	Chai	0.00	0.00	154	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
31	28	Centella TC Cream	Tuýp	0.00	0.00	1201	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	4	\N	\N	\N	\N	\N	\N	0.00	\N	f
32	29	Meso Calming Ampoule Mask II	Hộp	0.00	0.00	2878	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
33	3	Antioxidant Serum	Hộp	0.00	0.00	143	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
34	30	Skin Hydration Cleansing Gel	Chai	0.00	0.00	68	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
35	31	Skin Hydration Gel Toner	Chai	0.00	0.00	403	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
36	32	PH Balance Cleansing Cream	Chai	0.00	0.00	230	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
37	33	Centella Calmimg Gel Cream	Hộp	0.00	0.00	26	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
38	34	Complex AC Ampoule	Hộp	0.00	0.00	53	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
39	35	Complex AC Cleanser	Chai	0.00	0.00	83	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
40	36	Trừ Cọc		0.00	0.00	0	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	t
41	37	Đầu Kim	Cái	0.00	0.00	367	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	1	\N	\N	\N	\N	\N	\N	0.00	\N	f
42	38	Meso C Cream	Hộp	0.00	0.00	72	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
43	39	Brightening Fills Ampoule	Hộp	0.00	0.00	130	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
44	4	SÁCH 33 NHÂN HIỆU CHỦ SPA THÀNH CÔNG	Cuốn	0.00	0.00	1	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	5	\N	\N	\N	\N	\N	\N	0.00	\N	f
45	40	Meso Fills Cream	Hộp	0.00	0.00	76	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
46	41	Wrinkle Care Eye Cream	Hộp	0.00	0.00	259	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
47	42	Anti Wrinkle Eye Mask	Hộp	0.00	0.00	96	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
48	43	Ethosome Astaxanthin	Hộp	0.00	0.00	1378	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
49	44	E.G.F Stem C	Hộp	0.00	0.00	667	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
50	45	Meso Hydro Mask (100ml)	Hộp	0.00	0.00	175	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
51	46	Multi- Vitamin B	Chai	0.00	0.00	103	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
52	47	Blue Energy	Chai	0.00	0.00	172	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
53	48	Time Reset	Chai	0.00	0.00	315	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
54	49	Multi- Vitamin C	Chai	0.00	0.00	211	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
55	5	Meso Filler Pro	Bộ	0.00	0.00	35	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	6	\N	\N	\N	\N	\N	\N	0.00	\N	f
56	50	Pink Energy	Chai	0.00	0.00	120	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
57	51	Skin Brightening ACT	Chai	0.00	0.00	55	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
58	52	Skin Boosting ACT	Chai	0.00	0.00	163	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
59	53	Mesotox Lipo Cocktail	Hộp	0.00	0.00	54	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
60	54	Mesotox Skin Booster Glutathion	Hộp	0.00	0.00	93	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
61	55	Mesotox Skin Booster PDRN	Hộp	0.00	0.00	106	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
62	56	Meso Peel CL4 100ml	Chai	0.00	0.00	163	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
63	57	Clear Clarifying Cream Mask	Hộp	0.00	0.00	246	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
64	58	Vita Radiance Cream Mask	Hộp	0.00	0.00	62	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
65	59	Vita Radiance Serum	Hộp	0.00	0.00	65	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
66	6	Ống Xilanh Pro	Cái	0.00	0.00	135	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	1	\N	\N	\N	\N	\N	\N	0.00	\N	f
67	60	Centella Soothing Serum	Hộp	0.00	0.00	182	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
68	61	Clear Clarifying Serum	Hộp	0.00	0.00	139	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
69	62	B5 Moisturizing Cream Mask	Hộp	0.00	0.00	101	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
70	63	B5 Moisturizing Serum	Hộp	0.00	0.00	155	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
71	64	Centella Cream Mask	Hộp	0.00	0.00	99	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
72	65	CỌC		0.00	0.00	0	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	t
73	66	Radiance Lha Peel Pad	Hộp	0.00	0.00	145	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
74	67	Azulene Care Mist	Chai	0.00	0.00	188	t	\N	\N	\N	CoGiamThue	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	3	\N	\N	\N	\N	\N	\N	0.00	\N	f
75	68	Honeybush Skinsolution	Hộp	0.00	0.00	519	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
76	69	DERMA MATRIX EXO-PN	Hộp	0.00	0.00	280	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
77	7	Nút Cao Su	Cái	0.00	0.00	398	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	1	\N	\N	\N	\N	\N	\N	0.00	\N	f
78	8	Đầu Kim Pro	Cái	0.00	0.00	71	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	1	\N	\N	\N	\N	\N	\N	0.00	\N	f
79	9	Mesotox Skin Booster Scalp	Hộp	0.00	0.00	106	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	VatTuHangHoa	\N	\N	2	\N	\N	\N	\N	\N	\N	0.00	\N	f
80	CPMH	Chi phí mua hàng		0.00	0.00	0	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	DichVu	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
81	KHACHSAN_PHI_PHUCVU	Phí phục vụ		0.00	0.00	0	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	DichVu	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
82	LPXD	Lệ phí xăng dầu		0.00	0.00	0	t	\N	\N	\N	ChuaXacDinh	\N	\N	\N	\N	\N	\N	f	0.00	0	DichVu	\N	\N	\N	\N	\N	\N	\N	\N	\N	0.00	\N	f
\.


--
-- Data for Name: receipt_entries; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.receipt_entries ("Id", "ReceiptId", "Description", "DebitAccount", "CreditAccount", "Amount", "SubjectCode", "SubjectName", "BankAccount", "SalesOrderId") FROM stdin;
\.


--
-- Data for Name: receipts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.receipts ("Id", "CustomerId", "PayerName", "Address", "PaymentReason", "CollectorEmployeeId", "Attachment", "Reference", "AccountingDate", "DocumentDate", "DocumentNumber", "CreatedAt") FROM stdin;
\.


--
-- Data for Name: sales_order_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_order_lines (id, sales_order_id, product_id, product_code, product_name, is_promotion, unit, quantity, unit_price, amount, receivable_account, revenue_account, discount_rate, tax_amount, tax_rate, is_amount_manual, warehouse_id, is_deposit_product) FROM stdin;
1	1	1	SP001	Skin Hydration Gel Toner	f	Chai	2	1200000.00	2400000.00	131	511	0.00	192000.00	8.00	f	4	f
2	1	3	SP003	Centella TC Cream	f	Hộp	3	650000.00	1852500.00	131	511	5.00	148200.00	8.00	f	4	f
3	2	4	SP004	Sữa rửa mặt Cocoon	f	Chai	5	280000.00	1400000.00	131	511	0.00	70000.00	5.00	f	4	f
4	2	5	SP005	Mặt nạ dưỡng ẩm	f	Miếng	10	45000.00	450000.00	131	511	0.00	45000.00	10.00	f	4	f
5	3	2	SP002	Time Reset Serum	f	Chai	1	2160000.00	1944000.00	131	511	10.00	155520.00	8.00	f	4	f
6	3	6	SP006	Kem chống nắng SPF50	f	Tuýp	2	420000.00	840000.00	131	511	0.00	67200.00	8.00	f	4	f
7	4	8	SP008	Tinh chất Vitamin C	f	Chai	2	1450000.00	2900000.00	131	511	0.00	0.00	0.00	f	4	f
8	4	7	SP007	Combo quà tặng dùng thử	t	Bộ	1	0.00	0.00	131	511	0.00	0.00	0.00	f	4	f
9	5	1	SP001	Skin Hydration Gel Toner	f	Chai	4	1200000.00	4800000.00	131	511	0.00	384000.00	8.00	f	4	f
10	5	5	SP005	Mặt nạ dưỡng ẩm	f	Miếng	20	45000.00	900000.00	131	511	0.00	90000.00	10.00	f	4	f
22	7	4	SP004	Sữa rửa mặt Cocoon	f	Chai	1	280000.00	280000.00	131	511	0.00	14000.00	5.00	f	4	f
23	6	3	SP003	Centella TC Cream	t	Hộp	1	0.00	0.00	131	511	0.00	0.00	0.00	f	4	f
24	6	7	SP007	Combo quà tặng dùng thử	t	Bộ	1	0.00	0.00	131	511	0.00	0.00	0.00	f	4	f
25	8	1	SP001	Skin Hydration Gel Toner	f	Chai	1	1200000.00	1200000.00	131	511	0.00	96000.00	8.00	f	4	f
26	8	5	SP005	Mặt nạ dưỡng ẩm	t	Miếng	1	0.00	0.00	131	511	0.00	0.00	0.00	f	4	f
29	11	2	SP002	Time Reset Serum	f	Chai	1	2160000.00	2160000.00	131	511	0.00	172800.00	8.00	f	4	f
30	9	4	SP004	Sữa rửa mặt Cocoon	t	Chai	1	0.00	0.00	131	511	0.00	0.00	0.00	f	4	f
31	9	8	SP008	Tinh chất Vitamin C	f	Chai	1	1450000.00	1450000.00	131	511	0.00	0.00	0.00	f	4	f
32	10	8	SP008	Tinh chất Vitamin C	f	Chai	1	1450000.00	1450000.00	131	511	0.00	0.00	0.00	f	4	f
33	12	7	SP007	Combo quà tặng dùng thử	f	Bộ	1	0.00	0.00	131	511	0.00	0.00	0.00	f	4	f
35	14	4	SP004	Sữa rửa mặt Cocoon	f	Chai	1	280000.00	0.00	131	511	0.00	0.00	5.00	t	4	f
34	13	10	DATCOC	Đặt cọc	f	Lần	1	20000000.00	20000000.00	131	511	0.00	0.00	0.00	f	4	t
36	15	72	65	CỌC	f		1	2000000.00	2000000.00	131	511	0.00	0.00	0.00	t	\N	t
37	16	10	DATCOC	Đặt cọc	f	Lần	1	2000000.00	2000000.00	131	511	0.00	0.00	0.00	t	\N	t
38	17	24	21	Soothing Massage Cream	f	Chai	1	10000000.00	10000000.00	131	511	0.00	0.00	0.00	t	4	f
39	18	3	SP003	Centella TC Cream	f	Hộp	1	650000.00	650000.00	131	511	0.00	52000.00	8.00	f	4	f
40	19	24	21	Soothing Massage Cream	f	Chai	1	100000.00	100000.00	131	511	0.00	0.00	0.00	t	4	f
41	20	24	21	Soothing Massage Cream	f	Chai	1	1000000.00	1000000.00	131	511	0.00	0.00	0.00	t	4	f
42	21	3	SP003	Centella TC Cream	f	Hộp	1	650000.00	650000.00	131	511	0.00	52000.00	8.00	f	4	f
43	22	24	21	Soothing Massage Cream	f	Chai	1	10000.00	10000.00	131	511	0.00	0.00	0.00	t	4	f
44	23	12	10	Meso Calming Ampoule Mask II 10M	f	Hộp	1	10000000.00	10000000.00	131	511	0.00	0.00	0.00	t	4	f
45	24	3	SP003	Centella TC Cream	f	Hộp	10	650000.00	6500000.00	131	511	0.00	520000.00	8.00	f	4	f
46	24	24	21	Soothing Massage Cream	f	Chai	1	1000000.00	1000000.00	131	511	0.00	0.00	0.00	t	4	f
47	25	3	SP003	Centella TC Cream	f	Hộp	10	650000.00	6500000.00	131	511	0.00	520000.00	8.00	f	4	f
48	26	24	21	Soothing Massage Cream	f	Chai	1	300000.00	300000.00	131	511	0.00	0.00	0.00	t	4	f
49	27	5	SP005	Mặt nạ dưỡng ẩm	f	Miếng	1	45000.00	45000.00	131	511	0.00	4500.00	10.00	f	4	f
50	28	3	SP003	Centella TC Cream	f	Hộp	1	650000.00	650000.00	131	511	0.00	52000.00	8.00	f	4	f
\.


--
-- Data for Name: sales_orders; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_orders (id, document_number, accounting_date, document_date, customer_id, employee_id, description, reference, payment_terms, payment_due_days, payment_due_date, notes, delivery_method, payment_method, total_amount, created_at, status, grand_total, total_tax_amount, customer_name_override, customer_address_override) FROM stdin;
28	BH00005	2026-08-21 07:00:00+07	2026-08-21 07:00:00+07	1	2	Bán hàng demo	\N	\N	\N	\N	\N	\N	\N	650000.00	2026-08-22 11:39:52.5051+07	0	702000.00	52000.00	demo	\N
1	XK00001	2026-07-02 15:00:00+07	2026-07-02 15:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	4252500.00	2026-07-18 04:58:18.104896+07	0	4592700.00	340200.00	\N	\N
2	XK00002	2026-07-05 15:00:00+07	2026-07-05 15:00:00+07	2	3	Bán hàng CHI NHI COSMETICS	\N	\N	\N	\N	\N	\N	\N	1850000.00	2026-07-18 04:58:18.104896+07	0	1965000.00	115000.00	\N	\N
3	XK00003	2026-07-10 15:00:00+07	2026-07-10 15:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	2784000.00	2026-07-18 04:58:18.104896+07	0	3006720.00	222720.00	\N	\N
4	XK00004	2026-07-14 15:00:00+07	2026-07-14 15:00:00+07	3	4	Bán hàng NGỌC ANH SALON	\N	\N	\N	\N	\N	\N	\N	2900000.00	2026-07-18 04:58:18.104896+07	0	2900000.00	0.00	\N	\N
5	XK00005	2026-07-16 15:00:00+07	2026-07-16 15:00:00+07	4	5	Bán hàng HOÀNG GIA SPA & CLINIC	\N	\N	\N	\N	\N	\N	\N	5700000.00	2026-07-18 04:58:18.104896+07	0	6174000.00	474000.00	\N	\N
7	XK00007	2026-07-23 07:00:00+07	2026-07-23 07:00:00+07	2	3	Bán hàng CHI NHI COSMETICS	\N	\N	\N	\N	\N	\N	\N	280000.00	2026-07-25 15:52:19.007019+07	0	294000.00	14000.00	\N	\N
6	XK00006	2026-07-24 07:00:00+07	2026-07-24 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	0.00	2026-07-25 15:48:02.356293+07	0	0.00	0.00	\N	\N
8	XK00008	2026-07-23 07:00:00+07	2026-07-23 07:00:00+07	4	2	Bán hàng HOÀNG GIA SPA & CLINIC	\N	\N	\N	\N	\N	\N	\N	1200000.00	2026-07-25 15:52:54.398638+07	0	1296000.00	96000.00	\N	\N
11	XK00011	2026-07-29 07:00:00+07	2026-07-29 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	2160000.00	2026-07-31 09:29:01.872846+07	0	2332800.00	172800.00	\N	\N
9	XK00009	2026-07-24 07:00:00+07	2026-07-24 07:00:00+07	4	2	Bán hàng HOÀNG GIA SPA & CLINIC	\N	\N	\N	\N	\N	\N	\N	1450000.00	2026-07-25 15:53:25.002318+07	1	1450000.00	0.00	\N	\N
10	XK00010	2026-07-29 07:00:00+07	2026-07-29 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	1450000.00	2026-07-30 20:57:04.849404+07	0	1450000.00	0.00	\N	\N
12	XK00012	2026-07-29 07:00:00+07	2026-07-29 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	0.00	2026-07-31 09:35:25.07385+07	0	0.00	0.00	\N	\N
13	XK00013	2026-08-14 07:00:00+07	2026-08-14 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	20000000.00	2026-08-15 11:06:57.993424+07	0	20000000.00	0.00	\N	\N
14	XK00014	2026-08-14 07:00:00+07	2026-08-14 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	0.00	2026-08-15 11:08:11.814957+07	0	0.00	0.00	\N	\N
15	XK00015	2026-08-19 07:00:00+07	2026-08-19 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	2000000.00	2026-08-19 20:16:43.967934+07	0	2000000.00	0.00	\N	\N
16	XK00016	2026-08-19 07:00:00+07	2026-08-19 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	2000000.00	2026-08-19 20:28:28.252301+07	0	2000000.00	0.00	\N	\N
17	XK00017	2026-08-20 07:00:00+07	2026-08-20 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	10000000.00	2026-08-20 20:06:13.572566+07	0	10000000.00	0.00	\N	\N
18	XK00018	2026-08-20 07:00:00+07	2026-08-20 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	650000.00	2026-08-20 20:13:19.66911+07	0	702000.00	52000.00	\N	\N
19	XK00019	2026-08-20 07:00:00+07	2026-08-20 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	100000.00	2026-08-20 20:19:39.220185+07	0	100000.00	0.00	\N	\N
20	XK00020	2026-08-20 07:00:00+07	2026-08-20 07:00:00+07	2	3	Bán hàng CHI NHI COSMETICS	\N	\N	\N	\N	\N	\N	\N	1000000.00	2026-08-20 20:28:52.572226+07	0	1000000.00	0.00	\N	\N
21	XK00021	2026-08-20 07:00:00+07	2026-08-20 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	650000.00	2026-08-20 20:35:19.979537+07	0	702000.00	52000.00	\N	\N
22	XK00022	2026-08-20 07:00:00+07	2026-08-20 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	10000.00	2026-08-20 20:47:37.987804+07	0	10000.00	0.00	\N	\N
23	XK00023	2026-08-20 07:00:00+07	2026-08-20 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	10000000.00	2026-08-20 20:51:18.972171+07	0	10000000.00	0.00	\N	\N
24	BH00001	2026-08-21 07:00:00+07	2026-08-21 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	7500000.00	2026-08-22 10:39:11.210119+07	0	8020000.00	520000.00	\N	\N
25	BH00002	2026-08-21 07:00:00+07	2026-08-21 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	6500000.00	2026-08-22 10:45:48.114311+07	0	7020000.00	520000.00	\N	\N
26	BH00003	2026-08-21 07:00:00+07	2026-08-21 07:00:00+07	1	2	Bán hàng PHƯƠNG HOA SPA	\N	\N	\N	\N	\N	\N	\N	300000.00	2026-08-22 10:52:53.382834+07	0	300000.00	0.00	\N	\N
27	BH00004	2026-08-21 07:00:00+07	2026-08-21 07:00:00+07	1	2	Bán hàng xuat demo	\N	\N	\N	\N	\N	\N	\N	45000.00	2026-08-22 11:23:45.584927+07	0	49500.00	4500.00	xuat demo	\N
\.


--
-- Data for Name: sales_return_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_return_lines (id, sales_return_id, product_id, product_code, product_name, return_account, debt_account, discount_account, unit, quantity, unit_price, amount, discount_rate, discount_amount, sales_order_number, warehouse_id) FROM stdin;
1	1	3	SP003	Centella TC Cream	5212	131	5211	Hộp	1	650000.00	650000.00	5.00	32500.00	\N	4
\.


--
-- Data for Name: sales_returns; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sales_returns (id, document_number, accounting_date, document_date, customer_id, employee_id, description, reference, return_type, total_amount, total_discount, total_payment, created_at) FROM stdin;
1	BTL00001	2026-07-11 15:00:00+07	2026-07-11 15:00:00+07	1	2	Trả lại hàng PHƯƠNG HOA SPA	\N	0	650000.00	32500.00	617500.00	2026-07-18 04:58:18.104896+07
\.


--
-- Data for Name: suppliers; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.suppliers (id, code, name, address, "group", tax_code, phone, is_stop_tracking) FROM stdin;
1	NCC001	Công ty TNHH Mỹ phẩm Việt Hàn	12 Trường Sơn, P.15, Q.10, TP.HCM	Mỹ phẩm nhập khẩu	0312223344	0281234567	f
2	NCC002	Công ty CP Thiết bị Spa Sài Gòn	88 Cộng Hòa, P.4, Tân Bình, TP.HCM	Thiết bị Spa	0313334455	0289876543	f
3	NCC003	Nhà phân phối Dược mỹ phẩm An Khang	45 Lý Thường Kiệt, Q.10, TP.HCM	Dược mỹ phẩm	0314445566	0287654321	t
\.


--
-- Data for Name: warehouse_receipt_lines; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.warehouse_receipt_lines (id, warehouse_receipt_id, product_id, warehouse_id, quantity, unit_price, amount, debit_account, credit_account, cost_item, cost_object, loan_contract_number, project, purchase_order_number, sales_contract_number, statistics_code) FROM stdin;
1	1	6	1	1	180000.00	180000.00	111	131	\N	\N	\N	\N	\N	\N	\N
\.


--
-- Data for Name: warehouse_receipts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.warehouse_receipts (id, receipt_number, receipt_type, status, customer_id, employee_id, accounting_date, document_date, description, delivery_person, reference, total_amount, created_at, confirmed_at, supplier_id) FROM stdin;
1	NK00001	1	1	\N	2	2026-08-14 07:00:00+07	2026-08-14 07:00:00+07	\N	\N	\N	180000.00	2026-08-15 09:54:49.09453+07	2026-08-15 09:54:49.35598+07	\N
\.


--
-- Data for Name: warehouses; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.warehouses (id, code, name, is_active) FROM stdin;
1	KHO01	Kho chính	t
3	KHO02	Kho chi nhánh Q.1	t
4	HH	Hàng hoá	t
5	TB	Trưng bày	t
\.


--
-- Name: account_settings_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.account_settings_id_seq', 44, false);


--
-- Name: backup_schedule_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.backup_schedule_id_seq', 2, false);


--
-- Name: cash_transactions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.cash_transactions_id_seq', 16, true);


--
-- Name: categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.categories_id_seq', 5, true);


--
-- Name: customers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.customers_id_seq', 7, true);


--
-- Name: departments_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.departments_id_seq', 9, false);


--
-- Name: deposit_deductions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.deposit_deductions_id_seq', 3, true);


--
-- Name: deposits_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.deposits_id_seq', 4, true);


--
-- Name: employees_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.employees_id_seq', 5, true);


--
-- Name: expense_categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.expense_categories_id_seq', 1, true);


--
-- Name: payment_entries_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."payment_entries_Id_seq"', 5, true);


--
-- Name: payments_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."payments_Id_seq"', 6, true);


--
-- Name: product_units_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.product_units_id_seq', 11, false);


--
-- Name: product_warehouse_stocks_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.product_warehouse_stocks_id_seq', 105, true);


--
-- Name: products_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.products_id_seq', 82, true);


--
-- Name: receipt_entries_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."receipt_entries_Id_seq"', 1, false);


--
-- Name: receipts_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."receipts_Id_seq"', 1, false);


--
-- Name: sales_order_lines_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."sales_order_lines_Id_seq"', 50, true);


--
-- Name: sales_orders_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public."sales_orders_Id_seq"', 28, true);


--
-- Name: sales_return_lines_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sales_return_lines_id_seq', 1, true);


--
-- Name: sales_returns_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sales_returns_id_seq', 1, true);


--
-- Name: suppliers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.suppliers_id_seq', 3, true);


--
-- Name: warehouse_receipt_lines_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.warehouse_receipt_lines_id_seq', 1, true);


--
-- Name: warehouse_receipts_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.warehouse_receipts_id_seq', 1, true);


--
-- Name: warehouses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.warehouses_id_seq', 6, false);


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: account_settings PK_account_settings; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.account_settings
    ADD CONSTRAINT "PK_account_settings" PRIMARY KEY (id);


--
-- Name: backup_schedule PK_backup_schedule; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.backup_schedule
    ADD CONSTRAINT "PK_backup_schedule" PRIMARY KEY (id);


--
-- Name: cash_transactions PK_cash_transactions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.cash_transactions
    ADD CONSTRAINT "PK_cash_transactions" PRIMARY KEY (id);


--
-- Name: categories PK_categories; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.categories
    ADD CONSTRAINT "PK_categories" PRIMARY KEY (id);


--
-- Name: customers PK_customers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT "PK_customers" PRIMARY KEY (id);


--
-- Name: departments PK_departments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT "PK_departments" PRIMARY KEY (id);


--
-- Name: deposit_deductions PK_deposit_deductions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.deposit_deductions
    ADD CONSTRAINT "PK_deposit_deductions" PRIMARY KEY (id);


--
-- Name: deposits PK_deposits; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.deposits
    ADD CONSTRAINT "PK_deposits" PRIMARY KEY (id);


--
-- Name: employees PK_employees; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT "PK_employees" PRIMARY KEY (id);


--
-- Name: expense_categories PK_expense_categories; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.expense_categories
    ADD CONSTRAINT "PK_expense_categories" PRIMARY KEY (id);


--
-- Name: payment_entries PK_payment_entries; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payment_entries
    ADD CONSTRAINT "PK_payment_entries" PRIMARY KEY ("Id");


--
-- Name: payments PK_payments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT "PK_payments" PRIMARY KEY ("Id");


--
-- Name: product_units PK_product_units; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_units
    ADD CONSTRAINT "PK_product_units" PRIMARY KEY (id);


--
-- Name: product_warehouse_stocks PK_product_warehouse_stocks; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_warehouse_stocks
    ADD CONSTRAINT "PK_product_warehouse_stocks" PRIMARY KEY (id);


--
-- Name: products PK_products; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "PK_products" PRIMARY KEY (id);


--
-- Name: receipt_entries PK_receipt_entries; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.receipt_entries
    ADD CONSTRAINT "PK_receipt_entries" PRIMARY KEY ("Id");


--
-- Name: receipts PK_receipts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.receipts
    ADD CONSTRAINT "PK_receipts" PRIMARY KEY ("Id");


--
-- Name: sales_order_lines PK_sales_order_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_order_lines
    ADD CONSTRAINT "PK_sales_order_lines" PRIMARY KEY (id);


--
-- Name: sales_orders PK_sales_orders; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT "PK_sales_orders" PRIMARY KEY (id);


--
-- Name: sales_return_lines PK_sales_return_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT "PK_sales_return_lines" PRIMARY KEY (id);


--
-- Name: sales_returns PK_sales_returns; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT "PK_sales_returns" PRIMARY KEY (id);


--
-- Name: suppliers PK_suppliers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.suppliers
    ADD CONSTRAINT "PK_suppliers" PRIMARY KEY (id);


--
-- Name: warehouse_receipt_lines PK_warehouse_receipt_lines; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_receipt_lines
    ADD CONSTRAINT "PK_warehouse_receipt_lines" PRIMARY KEY (id);


--
-- Name: warehouse_receipts PK_warehouse_receipts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_receipts
    ADD CONSTRAINT "PK_warehouse_receipts" PRIMARY KEY (id);


--
-- Name: warehouses PK_warehouses; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouses
    ADD CONSTRAINT "PK_warehouses" PRIMARY KEY (id);


--
-- Name: IX_account_settings_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_account_settings_code" ON public.account_settings USING btree (code);


--
-- Name: IX_cash_transactions_accounting_date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_cash_transactions_accounting_date" ON public.cash_transactions USING btree (accounting_date);


--
-- Name: IX_categories_name; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_categories_name" ON public.categories USING btree (name);


--
-- Name: IX_customers_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_customers_code" ON public.customers USING btree (code);


--
-- Name: IX_customers_sale_care_employee_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_customers_sale_care_employee_id" ON public.customers USING btree (sale_care_employee_id);


--
-- Name: IX_departments_name; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_departments_name" ON public.departments USING btree (name);


--
-- Name: IX_deposit_deductions_accounting_date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_deposit_deductions_accounting_date" ON public.deposit_deductions USING btree (accounting_date);


--
-- Name: IX_deposit_deductions_deposit_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_deposit_deductions_deposit_id" ON public.deposit_deductions USING btree (deposit_id);


--
-- Name: IX_deposit_deductions_document_number; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_deposit_deductions_document_number" ON public.deposit_deductions USING btree (document_number);


--
-- Name: IX_deposit_deductions_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_deposit_deductions_sales_order_id" ON public.deposit_deductions USING btree (sales_order_id);


--
-- Name: IX_deposits_accounting_date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_deposits_accounting_date" ON public.deposits USING btree (accounting_date);


--
-- Name: IX_deposits_customer_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_deposits_customer_id" ON public.deposits USING btree (customer_id);


--
-- Name: IX_deposits_document_number; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_deposits_document_number" ON public.deposits USING btree (document_number);


--
-- Name: IX_deposits_employee_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_deposits_employee_id" ON public.deposits USING btree (employee_id);


--
-- Name: IX_deposits_source_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_deposits_source_sales_order_id" ON public.deposits USING btree (source_sales_order_id);


--
-- Name: IX_employees_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_employees_code" ON public.employees USING btree (code);


--
-- Name: IX_expense_categories_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_expense_categories_code" ON public.expense_categories USING btree (code);


--
-- Name: IX_expense_categories_department_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_expense_categories_department_id" ON public.expense_categories USING btree (department_id);


--
-- Name: IX_payment_entries_CreditAccountSettingId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_payment_entries_CreditAccountSettingId" ON public.payment_entries USING btree ("CreditAccountSettingId");


--
-- Name: IX_payment_entries_DebitAccountSettingId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_payment_entries_DebitAccountSettingId" ON public.payment_entries USING btree ("DebitAccountSettingId");


--
-- Name: IX_payment_entries_ExpenseCategoryId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_payment_entries_ExpenseCategoryId" ON public.payment_entries USING btree ("ExpenseCategoryId");


--
-- Name: IX_payment_entries_PaymentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_payment_entries_PaymentId" ON public.payment_entries USING btree ("PaymentId");


--
-- Name: IX_payments_PaymentEmployeeId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_payments_PaymentEmployeeId" ON public.payments USING btree ("PaymentEmployeeId");


--
-- Name: IX_payments_SupplierId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_payments_SupplierId" ON public.payments USING btree ("SupplierId");


--
-- Name: IX_product_units_name; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_product_units_name" ON public.product_units USING btree (name);


--
-- Name: IX_product_warehouse_stocks_product_id_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_product_warehouse_stocks_product_id_warehouse_id" ON public.product_warehouse_stocks USING btree (product_id, warehouse_id);


--
-- Name: IX_product_warehouse_stocks_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_product_warehouse_stocks_warehouse_id" ON public.product_warehouse_stocks USING btree (warehouse_id);


--
-- Name: IX_products_category_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_category_id" ON public.products USING btree (category_id);


--
-- Name: IX_products_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_products_code" ON public.products USING btree (code) WHERE ((code)::text <> ''::text);


--
-- Name: IX_products_cost_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_cost_account_id" ON public.products USING btree (cost_account_id);


--
-- Name: IX_products_default_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_default_warehouse_id" ON public.products USING btree (default_warehouse_id);


--
-- Name: IX_products_discount_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_discount_account_id" ON public.products USING btree (discount_account_id);


--
-- Name: IX_products_price_reduction_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_price_reduction_account_id" ON public.products USING btree (price_reduction_account_id);


--
-- Name: IX_products_product_unit_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_product_unit_id" ON public.products USING btree (product_unit_id);


--
-- Name: IX_products_return_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_return_account_id" ON public.products USING btree (return_account_id);


--
-- Name: IX_products_revenue_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_revenue_account_id" ON public.products USING btree (revenue_account_id);


--
-- Name: IX_products_stock_account_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_products_stock_account_id" ON public.products USING btree (stock_account_id);


--
-- Name: IX_receipt_entries_ReceiptId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_receipt_entries_ReceiptId" ON public.receipt_entries USING btree ("ReceiptId");


--
-- Name: IX_receipt_entries_SalesOrderId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_receipt_entries_SalesOrderId" ON public.receipt_entries USING btree ("SalesOrderId");


--
-- Name: IX_receipts_CollectorEmployeeId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_receipts_CollectorEmployeeId" ON public.receipts USING btree ("CollectorEmployeeId");


--
-- Name: IX_receipts_CustomerId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_receipts_CustomerId" ON public.receipts USING btree ("CustomerId");


--
-- Name: IX_sales_order_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_order_lines_product_id" ON public.sales_order_lines USING btree (product_id);


--
-- Name: IX_sales_order_lines_sales_order_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_order_lines_sales_order_id" ON public.sales_order_lines USING btree (sales_order_id);


--
-- Name: IX_sales_order_lines_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_order_lines_warehouse_id" ON public.sales_order_lines USING btree (warehouse_id);


--
-- Name: IX_sales_orders_accounting_date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_orders_accounting_date" ON public.sales_orders USING btree (accounting_date);


--
-- Name: IX_sales_orders_customer_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_orders_customer_id" ON public.sales_orders USING btree (customer_id);


--
-- Name: IX_sales_orders_document_number; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_sales_orders_document_number" ON public.sales_orders USING btree (document_number);


--
-- Name: IX_sales_orders_employee_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_orders_employee_id" ON public.sales_orders USING btree (employee_id);


--
-- Name: IX_sales_return_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_return_lines_product_id" ON public.sales_return_lines USING btree (product_id);


--
-- Name: IX_sales_return_lines_sales_return_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_return_lines_sales_return_id" ON public.sales_return_lines USING btree (sales_return_id);


--
-- Name: IX_sales_return_lines_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_return_lines_warehouse_id" ON public.sales_return_lines USING btree (warehouse_id);


--
-- Name: IX_sales_returns_accounting_date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_returns_accounting_date" ON public.sales_returns USING btree (accounting_date);


--
-- Name: IX_sales_returns_customer_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_returns_customer_id" ON public.sales_returns USING btree (customer_id);


--
-- Name: IX_sales_returns_document_number; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_sales_returns_document_number" ON public.sales_returns USING btree (document_number);


--
-- Name: IX_sales_returns_employee_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_sales_returns_employee_id" ON public.sales_returns USING btree (employee_id);


--
-- Name: IX_suppliers_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_suppliers_code" ON public.suppliers USING btree (code);


--
-- Name: IX_warehouse_receipt_lines_product_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_warehouse_receipt_lines_product_id" ON public.warehouse_receipt_lines USING btree (product_id);


--
-- Name: IX_warehouse_receipt_lines_warehouse_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_warehouse_receipt_lines_warehouse_id" ON public.warehouse_receipt_lines USING btree (warehouse_id);


--
-- Name: IX_warehouse_receipt_lines_warehouse_receipt_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_warehouse_receipt_lines_warehouse_receipt_id" ON public.warehouse_receipt_lines USING btree (warehouse_receipt_id);


--
-- Name: IX_warehouse_receipts_accounting_date; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_warehouse_receipts_accounting_date" ON public.warehouse_receipts USING btree (accounting_date);


--
-- Name: IX_warehouse_receipts_customer_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_warehouse_receipts_customer_id" ON public.warehouse_receipts USING btree (customer_id);


--
-- Name: IX_warehouse_receipts_employee_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_warehouse_receipts_employee_id" ON public.warehouse_receipts USING btree (employee_id);


--
-- Name: IX_warehouse_receipts_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_warehouse_receipts_status" ON public.warehouse_receipts USING btree (status);


--
-- Name: IX_warehouse_receipts_supplier_id; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_warehouse_receipts_supplier_id" ON public.warehouse_receipts USING btree (supplier_id);


--
-- Name: IX_warehouses_code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_warehouses_code" ON public.warehouses USING btree (code);


--
-- Name: customers FK_customers_employees_sale_care_employee_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT "FK_customers_employees_sale_care_employee_id" FOREIGN KEY (sale_care_employee_id) REFERENCES public.employees(id) ON DELETE SET NULL;


--
-- Name: deposit_deductions FK_deposit_deductions_deposits_deposit_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.deposit_deductions
    ADD CONSTRAINT "FK_deposit_deductions_deposits_deposit_id" FOREIGN KEY (deposit_id) REFERENCES public.deposits(id) ON DELETE RESTRICT;


--
-- Name: deposit_deductions FK_deposit_deductions_sales_orders_sales_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.deposit_deductions
    ADD CONSTRAINT "FK_deposit_deductions_sales_orders_sales_order_id" FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(id) ON DELETE RESTRICT;


--
-- Name: deposits FK_deposits_customers_customer_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.deposits
    ADD CONSTRAINT "FK_deposits_customers_customer_id" FOREIGN KEY (customer_id) REFERENCES public.customers(id) ON DELETE RESTRICT;


--
-- Name: deposits FK_deposits_employees_employee_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.deposits
    ADD CONSTRAINT "FK_deposits_employees_employee_id" FOREIGN KEY (employee_id) REFERENCES public.employees(id) ON DELETE SET NULL;


--
-- Name: deposits FK_deposits_sales_orders_source_sales_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.deposits
    ADD CONSTRAINT "FK_deposits_sales_orders_source_sales_order_id" FOREIGN KEY (source_sales_order_id) REFERENCES public.sales_orders(id) ON DELETE RESTRICT;


--
-- Name: expense_categories FK_expense_categories_departments_department_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.expense_categories
    ADD CONSTRAINT "FK_expense_categories_departments_department_id" FOREIGN KEY (department_id) REFERENCES public.departments(id) ON DELETE SET NULL;


--
-- Name: payment_entries FK_payment_entries_account_settings_CreditAccountSettingId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payment_entries
    ADD CONSTRAINT "FK_payment_entries_account_settings_CreditAccountSettingId" FOREIGN KEY ("CreditAccountSettingId") REFERENCES public.account_settings(id) ON DELETE RESTRICT;


--
-- Name: payment_entries FK_payment_entries_account_settings_DebitAccountSettingId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payment_entries
    ADD CONSTRAINT "FK_payment_entries_account_settings_DebitAccountSettingId" FOREIGN KEY ("DebitAccountSettingId") REFERENCES public.account_settings(id) ON DELETE RESTRICT;


--
-- Name: payment_entries FK_payment_entries_expense_categories_ExpenseCategoryId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payment_entries
    ADD CONSTRAINT "FK_payment_entries_expense_categories_ExpenseCategoryId" FOREIGN KEY ("ExpenseCategoryId") REFERENCES public.expense_categories(id) ON DELETE SET NULL;


--
-- Name: payment_entries FK_payment_entries_payments_PaymentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payment_entries
    ADD CONSTRAINT "FK_payment_entries_payments_PaymentId" FOREIGN KEY ("PaymentId") REFERENCES public.payments("Id") ON DELETE CASCADE;


--
-- Name: payments FK_payments_employees_PaymentEmployeeId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT "FK_payments_employees_PaymentEmployeeId" FOREIGN KEY ("PaymentEmployeeId") REFERENCES public.employees(id) ON DELETE SET NULL;


--
-- Name: payments FK_payments_suppliers_SupplierId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT "FK_payments_suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES public.suppliers(id) ON DELETE RESTRICT;


--
-- Name: product_warehouse_stocks FK_product_warehouse_stocks_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_warehouse_stocks
    ADD CONSTRAINT "FK_product_warehouse_stocks_products_product_id" FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE;


--
-- Name: product_warehouse_stocks FK_product_warehouse_stocks_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product_warehouse_stocks
    ADD CONSTRAINT "FK_product_warehouse_stocks_warehouses_warehouse_id" FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: products FK_products_account_settings_cost_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_account_settings_cost_account_id" FOREIGN KEY (cost_account_id) REFERENCES public.account_settings(id) ON DELETE SET NULL;


--
-- Name: products FK_products_account_settings_discount_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_account_settings_discount_account_id" FOREIGN KEY (discount_account_id) REFERENCES public.account_settings(id) ON DELETE SET NULL;


--
-- Name: products FK_products_account_settings_price_reduction_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_account_settings_price_reduction_account_id" FOREIGN KEY (price_reduction_account_id) REFERENCES public.account_settings(id) ON DELETE SET NULL;


--
-- Name: products FK_products_account_settings_return_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_account_settings_return_account_id" FOREIGN KEY (return_account_id) REFERENCES public.account_settings(id) ON DELETE SET NULL;


--
-- Name: products FK_products_account_settings_revenue_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_account_settings_revenue_account_id" FOREIGN KEY (revenue_account_id) REFERENCES public.account_settings(id) ON DELETE SET NULL;


--
-- Name: products FK_products_account_settings_stock_account_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_account_settings_stock_account_id" FOREIGN KEY (stock_account_id) REFERENCES public.account_settings(id) ON DELETE SET NULL;


--
-- Name: products FK_products_categories_category_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_categories_category_id" FOREIGN KEY (category_id) REFERENCES public.categories(id) ON DELETE RESTRICT;


--
-- Name: products FK_products_product_units_product_unit_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_product_units_product_unit_id" FOREIGN KEY (product_unit_id) REFERENCES public.product_units(id) ON DELETE SET NULL;


--
-- Name: products FK_products_warehouses_default_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.products
    ADD CONSTRAINT "FK_products_warehouses_default_warehouse_id" FOREIGN KEY (default_warehouse_id) REFERENCES public.warehouses(id) ON DELETE SET NULL;


--
-- Name: receipt_entries FK_receipt_entries_receipts_ReceiptId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.receipt_entries
    ADD CONSTRAINT "FK_receipt_entries_receipts_ReceiptId" FOREIGN KEY ("ReceiptId") REFERENCES public.receipts("Id") ON DELETE CASCADE;


--
-- Name: receipt_entries FK_receipt_entries_sales_orders_SalesOrderId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.receipt_entries
    ADD CONSTRAINT "FK_receipt_entries_sales_orders_SalesOrderId" FOREIGN KEY ("SalesOrderId") REFERENCES public.sales_orders(id) ON DELETE RESTRICT;


--
-- Name: receipts FK_receipts_customers_CustomerId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.receipts
    ADD CONSTRAINT "FK_receipts_customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES public.customers(id) ON DELETE RESTRICT;


--
-- Name: receipts FK_receipts_employees_CollectorEmployeeId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.receipts
    ADD CONSTRAINT "FK_receipts_employees_CollectorEmployeeId" FOREIGN KEY ("CollectorEmployeeId") REFERENCES public.employees(id) ON DELETE SET NULL;


--
-- Name: sales_order_lines FK_sales_order_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_order_lines
    ADD CONSTRAINT "FK_sales_order_lines_products_product_id" FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: sales_order_lines FK_sales_order_lines_sales_orders_sales_order_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_order_lines
    ADD CONSTRAINT "FK_sales_order_lines_sales_orders_sales_order_id" FOREIGN KEY (sales_order_id) REFERENCES public.sales_orders(id) ON DELETE CASCADE;


--
-- Name: sales_order_lines FK_sales_order_lines_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_order_lines
    ADD CONSTRAINT "FK_sales_order_lines_warehouses_warehouse_id" FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: sales_orders FK_sales_orders_customers_customer_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT "FK_sales_orders_customers_customer_id" FOREIGN KEY (customer_id) REFERENCES public.customers(id) ON DELETE RESTRICT;


--
-- Name: sales_orders FK_sales_orders_employees_employee_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_orders
    ADD CONSTRAINT "FK_sales_orders_employees_employee_id" FOREIGN KEY (employee_id) REFERENCES public.employees(id) ON DELETE SET NULL;


--
-- Name: sales_return_lines FK_sales_return_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT "FK_sales_return_lines_products_product_id" FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: sales_return_lines FK_sales_return_lines_sales_returns_sales_return_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT "FK_sales_return_lines_sales_returns_sales_return_id" FOREIGN KEY (sales_return_id) REFERENCES public.sales_returns(id) ON DELETE CASCADE;


--
-- Name: sales_return_lines FK_sales_return_lines_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_return_lines
    ADD CONSTRAINT "FK_sales_return_lines_warehouses_warehouse_id" FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: sales_returns FK_sales_returns_customers_customer_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT "FK_sales_returns_customers_customer_id" FOREIGN KEY (customer_id) REFERENCES public.customers(id) ON DELETE RESTRICT;


--
-- Name: sales_returns FK_sales_returns_employees_employee_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sales_returns
    ADD CONSTRAINT "FK_sales_returns_employees_employee_id" FOREIGN KEY (employee_id) REFERENCES public.employees(id) ON DELETE SET NULL;


--
-- Name: warehouse_receipt_lines FK_warehouse_receipt_lines_products_product_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_receipt_lines
    ADD CONSTRAINT "FK_warehouse_receipt_lines_products_product_id" FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT;


--
-- Name: warehouse_receipt_lines FK_warehouse_receipt_lines_warehouse_receipts_warehouse_receip~; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_receipt_lines
    ADD CONSTRAINT "FK_warehouse_receipt_lines_warehouse_receipts_warehouse_receip~" FOREIGN KEY (warehouse_receipt_id) REFERENCES public.warehouse_receipts(id) ON DELETE CASCADE;


--
-- Name: warehouse_receipt_lines FK_warehouse_receipt_lines_warehouses_warehouse_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_receipt_lines
    ADD CONSTRAINT "FK_warehouse_receipt_lines_warehouses_warehouse_id" FOREIGN KEY (warehouse_id) REFERENCES public.warehouses(id) ON DELETE RESTRICT;


--
-- Name: warehouse_receipts FK_warehouse_receipts_customers_customer_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_receipts
    ADD CONSTRAINT "FK_warehouse_receipts_customers_customer_id" FOREIGN KEY (customer_id) REFERENCES public.customers(id) ON DELETE RESTRICT;


--
-- Name: warehouse_receipts FK_warehouse_receipts_employees_employee_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_receipts
    ADD CONSTRAINT "FK_warehouse_receipts_employees_employee_id" FOREIGN KEY (employee_id) REFERENCES public.employees(id) ON DELETE SET NULL;


--
-- Name: warehouse_receipts FK_warehouse_receipts_suppliers_supplier_id; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.warehouse_receipts
    ADD CONSTRAINT "FK_warehouse_receipts_suppliers_supplier_id" FOREIGN KEY (supplier_id) REFERENCES public.suppliers(id) ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

\unrestrict 926hdm9lo7X0dXRrQznYNb5xt8OpCQoPXtNRjRYCZtMFhAI0Bao2qedtSKWUaLq

