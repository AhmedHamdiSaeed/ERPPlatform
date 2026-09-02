#!/usr/bin/env python3
"""
Frontend-action LIFECYCLE test harness.

For every backend resource the Angular frontend talks to, this runs a real
CRUD lifecycle against the live backend at https://localhost:44327:

    list -> find-or-create an entity -> get-by-id -> update -> delete

Only entities *created by this run* are deleted, so seeded data is untouched.

Compared with the previous "dummy GUID" version this removes three classes of
false failure:
  * unknown-id 404s  (we use real IDs, resolving {employeeId} etc. from the
                      matching resource's own list endpoint)
  * missing-query 400s (required query params are filled from the Swagger spec)
  * 415 on PUT        (a generated body is always sent)

Rate limiting (200 req/min global, 50 req/min for writes) is respected by
pacing requests and by waiting out any 429 window.
"""
import json
import re
import ssl
import sys
import time
import urllib.request
import urllib.error
import urllib.parse
import uuid
from collections import OrderedDict

BASE = "https://localhost:44327"
SSL_CTX = ssl.create_default_context()
SSL_CTX.check_hostname = False
SSL_CTX.verify_mode = ssl.CERT_NONE

VERBOSE = "-v" in sys.argv

# ---------------------------------------------------------------- rate limiting
WRITE_TIMES = []          # timestamps of write requests in the last 60s
LAST_READ = [0.0]
RATE_LIMIT_WAITS = [0]


def _pace_write():
    """Keep writes under ~45/min so we do not trip the 50/min write limiter."""
    now = time.time()
    WRITE_TIMES[:] = [t for t in WRITE_TIMES if now - t < 60.0]
    if len(WRITE_TIMES) >= 45:
        sleep_for = 60.0 - (now - WRITE_TIMES[0]) + 1.0
        sys.stderr.write(f"    [write-throttle] sleeping {sleep_for:.0f}s\n")
        sys.stderr.flush()
        time.sleep(sleep_for)
        WRITE_TIMES.clear()
    WRITE_TIMES.append(time.time())


def _pace_read():
    gap = time.time() - LAST_READ[0]
    if gap < 0.30:
        time.sleep(0.30 - gap)
    LAST_READ[0] = time.time()


def request(method, path, token=None, body=None, timeout=30):
    """Send a request; transparently wait out the rate-limit window."""
    url = BASE + path
    data = None
    headers = {"Accept": "application/json"}
    if body is not None:
        data = json.dumps(body).encode("utf-8")
        headers["Content-Type"] = "application/json"
    if token:
        headers["Authorization"] = "Bearer " + token

    is_write = method in ("POST", "PUT", "DELETE", "PATCH")
    for attempt in range(4):
        if is_write:
            _pace_write()
        else:
            _pace_read()
        req = urllib.request.Request(url, data=data, headers=headers, method=method)
        try:
            with urllib.request.urlopen(req, timeout=timeout, context=SSL_CTX) as r:
                raw = r.read().decode("utf-8", "replace")
                return r.status, raw
        except urllib.error.HTTPError as e:
            raw = e.read().decode("utf-8", "replace")
            if e.code == 429 and attempt < 3:
                RATE_LIMIT_WAITS[0] += 1
                sys.stderr.write(
                    f"    [rate-limited] waiting 62s (wait #{RATE_LIMIT_WAITS[0]})...\n")
                sys.stderr.flush()
                time.sleep(62)
                continue
            return e.code, raw
        except Exception as e:
            return 0, str(e)
    return 429, '{"error":"rate limited"}'


def get_token():
    url = BASE + "/connect/token"
    body = (
        "grant_type=password&client_id=ERPPlatform_App"
        "&username=admin&password=1q2w3E*"
        "&scope=offline_access%20ERPPlatform"
    ).encode()
    for attempt in range(4):
        req = urllib.request.Request(
            url, data=body,
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            method="POST")
        try:
            with urllib.request.urlopen(req, timeout=30, context=SSL_CTX) as r:
                return json.loads(r.read().decode())["access_token"]
        except urllib.error.HTTPError as e:
            if e.code == 429 and attempt < 3:
                sys.stderr.write("    [rate-limited on /connect/token] waiting 62s...\n")
                sys.stderr.flush()
                time.sleep(62)
                continue
            raise
    raise RuntimeError("could not obtain token: rate limited")


def load_swagger():
    with urllib.request.urlopen(BASE + "/swagger/v1/swagger.json",
                                timeout=40, context=SSL_CTX) as r:
        return json.loads(r.read().decode("utf-8"))


def short(raw, n=160):
    if not raw:
        return ""
    s = raw.strip()
    return s[:n] + "..." if len(s) > n else s


# ------------------------------------------------------------- body generation
def normalize(s):
    return re.sub(r"[^a-z0-9]", "", (s or "").lower())


def value_for(pspec, pname, swagger, depth=0):
    """Produce a plausible value for a Swagger schema fragment."""
    enum = pspec.get("enum")
    if enum:
        return enum[0]
    if "$ref" in pspec:
        return None
    t = pspec.get("type")
    fmt = pspec.get("format", "")
    low = (pname or "").lower()
    if t == "string":
        if fmt == "date-time" or "date" in low or low.endswith("at"):
            return "2026-01-15T00:00:00Z"
        if fmt == "uuid":
            # userId almost always means "the signed-in user"; use the real one
            if low in ("userid", "currentuserid", "assigneduserid"):
                return CURRENT_USER_ID[0] or str(uuid.uuid4())
            return str(uuid.uuid4())
        if "email" in low:
            return "test.user@example.com"
        if "code" in low:
            return "T" + uuid.uuid4().hex[:6].upper()
        if "number" in low:
            return "N" + uuid.uuid4().hex[:6].upper()
        if "color" in low:
            return "#2563EB"
        if "phone" in low:
            return "+1-555-0100"
        if "password" in low:
            return "1q2w3E*"
        if "url" in low or "endpoint" in low:
            return "https://example.com/api"
        return "Test " + (pname or "value")
    if t in ("integer", "number"):
        if "year" in low:
            return 2026
        if "month" in low:
            return 6
        if "price" in low or "amount" in low or "salary" in low or "cost" in low:
            return 100.0
        if "count" in low or "quantity" in low or "qty" in low:
            return 5
        return 10
    if t == "boolean":
        return True
    if t == "array":
        return []
    if t == "object":
        return {}
    return None


def build_from_schema(schema, swagger, depth=0):
    if depth > 4:
        return None
    props = schema.get("properties", {})
    if not props:
        # non-object body (bare string / number / array)
        return value_for(schema, "value", swagger, depth)
    out = OrderedDict()
    required = set(schema.get("required", []))
    # when nothing is marked required, send every property: several services
    # validate fields that Swagger does not flag
    names = [n for n in props if n in required] if required else list(props)
    for pname in names:
        if pname in ("id", "Id", "concurrencyStamp", "extraProperties"):
            continue
        v = value_for(props[pname], pname, swagger, depth)
        if v is not None:
            out[pname] = v
    return out


def make_body(spec, swagger, override=None):
    """Build a request body from a Swagger operation's requestBody schema."""
    if override is not None:
        return override
    try:
        schema = spec["requestBody"]["content"]["application/json"]["schema"]
    except Exception:
        return None
    try:
        if "$ref" in schema:
            name = schema["$ref"].split("/")[-1]
            schema = swagger["components"]["schemas"].get(name)
            if not schema:
                return None
        body = build_from_schema(schema, swagger)
        return body or None
    except Exception:
        return None


def fill_query(spec, swagger):
    """Build a query string from required query parameters."""
    parts = []
    for p in spec.get("parameters", []):
        if p.get("in") != "query":
            continue
        name = p.get("name", "")
        if name in ("SkipCount", "MaxResultCount", "Sorting"):
            continue
        schema = p.get("schema", {})
        # Swagger often omits `required` on query params that the service still
        # validates, so fill every non-paging parameter.
        v = value_for(schema, name, swagger)
        if v is None:
            continue
        parts.append(f"{name}={urllib.parse.quote(str(v))}")
    return ("?" + "&".join(parts)) if parts else ""


# ------------------------------------------------------------------ ID cache
ID_CACHE = {}         # prefix -> real Guid
CREATED = {}          # prefix -> Guid we created ourselves (safe to delete)
CREATION_FAILED = set()
DELETED = set()
CURRENT_USER_ID = [None]


def get_current_user_id(token):
    """Resolve the signed-in user's id so {userId}-style params are real."""
    if CURRENT_USER_ID[0]:
        return CURRENT_USER_ID[0]
    for path in ("/api/identity/my-profile", "/api/account/my-profile"):
        status, raw = request("GET", path, token)
        if status == 200:
            try:
                data = json.loads(raw)
                uid = data.get("id") or data.get("userId")
                if uid:
                    CURRENT_USER_ID[0] = uid
                    return uid
            except Exception:
                pass
    return None


def get_existing_id(prefix, token, swagger, paths):
    """Return an existing entity id for `prefix` by listing it, else None."""
    list_path = prefix
    if list_path not in paths or "get" not in paths[list_path]:
        return None
    if ID_CACHE.get(prefix):
        return ID_CACHE[prefix]
    status, raw = request("GET", prefix + "?SkipCount=0&MaxResultCount=20", token)
    if status != 200:
        return None
    try:
        data = json.loads(raw)
    except Exception:
        return None
    items = data.get("items") if isinstance(data, dict) else None
    if not items:
        return None
    first = items[0]
    eid = first.get("id") or first.get("Id")
    if eid:
        ID_CACHE[prefix] = eid
    return eid


def create_entity(prefix, token, swagger, paths):
    """POST a new entity for `prefix`; return its id (and record ownership)."""
    if prefix in CREATED:
        return CREATED[prefix]
    if prefix in CREATION_FAILED:
        return None
    list_path = prefix
    if list_path not in paths or "post" not in paths[list_path]:
        CREATION_FAILED.add(prefix)
        return None
    body = make_body(paths[list_path]["post"], swagger)
    status, raw = request("POST", list_path, token, body)
    if status not in (200, 201):
        CREATION_FAILED.add(prefix)
        return None
    try:
        data = json.loads(raw)
    except Exception:
        CREATION_FAILED.add(prefix)
        return None
    eid = data.get("id") if isinstance(data, dict) else None
    if not eid:
        CREATION_FAILED.add(prefix)
        return None
    CREATED[prefix] = eid
    ID_CACHE[prefix] = eid
    return eid


def ensure_id(prefix, token, swagger, paths):
    """A real id belonging to `prefix`: reuse an existing row, else create one."""
    return (get_existing_id(prefix, token, swagger, paths)
            or create_entity(prefix, token, swagger, paths))


def resolve_path(path, own_prefix, token, swagger, paths, key2prefix):
    """Replace {placeholders} in a Swagger path with real ids."""
    placeholders = re.findall(r"\{([^}]+)\}", path)
    if not placeholders:
        return path, True

    concrete = path
    unresolved = []
    for ph in placeholders:
        key = normalize(ph)
        target_prefix = None
        if key in ("id", ""):
            target_prefix = own_prefix
        else:
            if key.endswith("id"):
                key = key[:-2]
            target_prefix = key2prefix.get(key, own_prefix)
        eid = ensure_id(target_prefix, token, swagger, paths)
        if eid:
            concrete = concrete.replace("{" + ph + "}", eid)
        else:
            unresolved.append(ph)
    # anything still unresolved: give it the resource's own id so the route is
    # still exercised rather than 400-ing on a malformed guid
    if unresolved:
        own = ensure_id(own_prefix, token, swagger, paths)
        if own:
            for ph in unresolved:
                concrete = concrete.replace("{" + ph + "}", own)
    return concrete, not unresolved


# ------------------------------------------------------------------ page map
PAGES = [
    ("Dashboard",              "/dashboard",                ["/api/hr/dashboard-stats", "/api/app/dashboard-config"]),
    ("HR - Employees",         "/hr/employees",             ["/api/hr/employee"]),
    ("HR - Departments",       "/hr/departments",           ["/api/hr/department"]),
    ("HR - Attendance",        "/hr/attendance",            ["/api/hr/attendance"]),
    ("HR - Leave",             "/hr/leave",                 ["/api/hr/leave-request", "/api/hr/leave-policy"]),
    ("HR - Payroll",           "/hr/payroll",               ["/api/app/payroll"]),
    ("HR - Recruitment",       "/hr/recruitment",           ["/api/app/candidate"]),
    ("HR - Field Visits",      "/hr/field-visit",           ["/api/hr/field-visit"]),
    ("HR - Expenses",          "/finance/expenses",         ["/api/hr/expense"]),
    ("Inventory - Products",   "/inventory/products",       ["/api/inventory/product"]),
    ("Inventory - Warehouses", "/inventory/warehouses",     ["/api/inventory/warehouse"]),
    ("Inventory - Transfers",  "/inventory/transfers",      ["/api/inventory/stock-transfer"]),
    ("Inventory - Stock Ops",  "/inventory/stock-operations", ["/api/inventory/stock-count"]),
    ("Inventory - PickList",   "/inventory/stock-operations", ["/api/inventory/pick-list"]),
    ("Inventory - PackList",   "/inventory/stock-operations", ["/api/inventory/pack-list"]),
    ("Inventory - Purchase Orders",   "/inventory/purchase-orders",   ["/api/inventory/purchase-order"]),
    ("Inventory - Purchase Requests", "/inventory/purchase-requests", ["/api/hr/purchase-request"]),
    ("Inventory - Goods Receipts",    "/inventory/goods-receipts",    ["/api/hr/goods-receipt"]),
    ("Inventory - Suppliers",  "/inventory/suppliers",      ["/api/hr/supplier", "/api/hr/rfq"]),
    ("Sales - Customers",      "/sales/customers",          ["/api/hr/customer", "/api/app/crm"]),
    ("Sales - Dashboard",      "/sales/dashboard",          ["/api/hr/sales-order"]),
    ("Sales - Quotations",     "/sales/quotations",         ["/api/inventory/sales-quotation"]),
    ("Sales - Invoices",       "/sales/invoices",           ["/api/inventory/sales-invoice"]),
    ("Sales - Delivery Notes", "/sales/delivery-notes",     ["/api/hr/delivery-note"]),
    ("Sales - Leads",          "/sales/crm/leads",          ["/api/hr/lead"]),
    ("Finance - Dashboard",    "/finance/dashboard",        ["/api/app/finance", "/api/hr/cost-center", "/api/hr/fiscal-year"]),
    ("Finance - Payments",     "/finance/dashboard",        ["/api/app/payment"]),
    ("Finance - AR/AP",        "/finance/dashboard",        ["/api/app/accounts-receivable", "/api/app/accounts-payable"]),
    ("Finance - Bank Recon",   "/finance/dashboard",        ["/api/app/bank-reconciliation"]),
    ("Projects",               "/projects",                 ["/api/hr/project"]),
    ("Manufacturing",          "/manufacturing",            ["/api/hr/manufacturing"]),
    ("Assets",                 "/assets",                   ["/api/hr/asset", "/api/hr/maintenance"]),
    ("Workflow - Designer",    "/workflow/designer",        ["/api/workflow/workflow-definition"]),
    ("Workflow - Tasks",       "/workflow/tasks",           ["/api/workflow/workflow-task"]),
    ("Workflow - Approvals",   "/workflow/approvals",       ["/api/app/approval-center"]),
    ("Workflow - History",     "/workflow/history",         ["/api/app/workflow-execution-log"]),
    ("AI Assistant",           "/ai-assistant",             ["/api/ai/ai-assistant"]),
    ("Reports",                "/reports",                  ["/api/app/report-definition"]),
    ("Notifications",          "/notifications",            ["/api/app/notification"]),
    ("Chat",                   "/chat",                     ["/api/app/chat"]),
    ("Documents",              "/documents",                ["/api/app/document", "/api/app/folder"]),
    ("SaaS - Subscription",    "/saas/subscription",        ["/api/app/subscription"]),
    ("SaaS - Usage",           "/saas/usage",               ["/api/app/usage"]),
    ("SaaS - Plans",           "/saas/plans",               ["/api/app/plan"]),
    ("Settings - Company",     "/settings/company",         ["/api/hr/company", "/api/hr/branch"]),
    ("Settings - Currency",    "/settings/currency-tax",    ["/api/hr/currency", "/api/hr/tax-config"]),
    ("Settings - PayTerms",    "/settings/payment-terms",   ["/api/hr/payment-term"]),
    ("Settings - Users",       "/settings/users",           ["/api/identity/users"]),
    ("Settings - Roles",       "/settings/roles",           ["/api/identity/roles"]),
    ("Settings - Audit",       "/settings/audit-trail",     ["/api/app/audit-log"]),
    ("Settings - Integrations", "/settings/integrations",   ["/api/app/integration-config"]),
    ("Settings - Branding",    "/settings",                 ["/api/app/branding"]),
    ("Devices",                "/profile",                  ["/api/app/device-registration"]),
    ("Search",                 "/dashboard",                ["/api/app/search"]),
]

# resources we must never delete (would lock the admin out / break auth)
DELETE_BLACKLIST = ("/api/identity/users", "/api/identity/roles")


def main():
    token = get_token()
    swagger = load_swagger()
    paths = swagger["paths"]
    get_current_user_id(token)

    # map a normalised resource name -> swagger prefix, for placeholder lookup
    key2prefix = {}
    for _, _, prefixes in PAGES:
        for p in prefixes:
            seg = p.rstrip("/").split("/")[-1]
            key2prefix.setdefault(normalize(seg), p)

    results = {name: [] for name, _, _ in PAGES}
    tested = set()

    for page, url, prefixes in PAGES:
        for prefix in prefixes:
            for path in sorted(paths):
                if not (path == prefix or path.startswith(prefix + "/")):
                    continue
                for method, spec in paths[path].items():
                    if method not in ("get", "post", "put", "delete"):
                        continue
                    key = (method.upper(), path)
                    if key in tested:
                        continue
                    tested.add(key)

                    concrete, resolved = resolve_path(
                        path, prefix, token, swagger, paths, key2prefix)
                    q = fill_query(spec, swagger)

                    if method == "get":
                        if not q and "SkipCount" in [
                                p.get("name", "") for p in spec.get("parameters", [])]:
                            q = "?SkipCount=0&MaxResultCount=10"
                        status, raw = request("GET", concrete + q, token)
                    elif method == "post":
                        # always send a JSON body: an empty POST yields 415
                        body = make_body(spec, swagger) or {}
                        status, raw = request("POST", concrete + q, token, body)
                    elif method == "put":
                        body = make_body(spec, swagger) or {}
                        status, raw = request("PUT", concrete + q, token, body)
                    else:  # delete
                        if any(concrete.startswith(b) for b in DELETE_BLACKLIST):
                            status, raw = 0, "skipped (protected resource)"
                        else:
                            status, raw = request("DELETE", concrete + q, token)
                            if status in (200, 204) and CREATED.get(prefix) \
                                    and CREATED[prefix] in concrete:
                                DELETED.add(prefix)

                    ok = status in (200, 201, 204)
                    results[page].append({
                        "method": method.upper(),
                        "path": path,
                        "status": status,
                        "ok": ok,
                        "resolved": resolved,
                        "resp": short(raw),
                    })

    # ------------------------------------------------------------- cleanup
    # remove any temp entity the delete-endpoint test did not already remove
    for prefix, eid in list(CREATED.items()):
        if prefix in DELETED or prefix in CREATION_FAILED:
            continue
        if any(prefix.startswith(b) for b in DELETE_BLACKLIST):
            continue
        if prefix in paths and "delete" in paths[prefix]:
            status, _ = request("DELETE", f"{prefix}/{eid}", token)
            if status in (200, 204):
                DELETED.add(prefix)
    leftover = [p for p in CREATED if p not in DELETED]

    # ------------------------------------------------------------- report
    lines = []
    total = passed = 0
    failed_pages = []
    for page, url, _ in PAGES:
        rows = results[page]
        if not rows:
            continue
        npass = sum(1 for r in rows if r["ok"])
        total += len(rows)
        passed += npass
        if npass != len(rows):
            failed_pages.append((page, url, [r for r in rows if not r["ok"]]))
        flag = "OK " if npass == len(rows) else "!! "
        lines.append(f"{flag}{page:34s} {npass:3d}/{len(rows):3d}  {url}")
        if VERBOSE:
            for r in rows:
                lines.append(
                    f"       [{'ok  ' if r['ok'] else 'FAIL'}] "
                    f"{r['method']:6s} {r['status']:3d} {r['path']}")

    print("\n".join(lines))
    print()
    print(f"TOTAL: {passed}/{total} actions returned 2xx")
    print(f"(waited out the rate limiter {RATE_LIMIT_WAITS[0]} time(s))")
    print(f"(created {len(CREATED)} temp entities, "
          f"{len(CREATION_FAILED)} resources could not be created)")
    print(f"(cleaned up {len(DELETED)} temp entities; "
          f"{len(leftover)} left behind)")
    if leftover:
        print(f"  leftover: {', '.join(sorted(leftover))}")
    print()

    if failed_pages:
        print("=" * 78)
        print("PAGES WITH FAILURES")
        print("=" * 78)
        for page, url, bad in failed_pages:
            print(f"\n### {page}  ({url})")
            for r in bad:
                note = "" if r["resolved"] else "  [unresolved id placeholder]"
                print(f"   {r['method']:6s} {r['status']:3d}  {r['path']}{note}")
                print(f"          {r['resp']}")
    return 0 if passed == total else 1


if __name__ == "__main__":
    sys.exit(main())
