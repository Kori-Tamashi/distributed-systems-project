# Known Issues and Technical Debt

## ValidateTicket Enum Consistency (Fixed)

**Status**: ✅ Fixed

**Issue**: `ValidateTicket` method in `TicketService.cs` used hardcoded values `1/2` for status validation, while `TicketStatus` enum defines `Confirmed=0, Cancelled=1, Refunded=2`.

**Impact**: `ValidateTicket` is called from `CreateAsync` and `UpdateAsync`, but NOT from `BuyTicketAsync`/`ReturnTicketAsync`. This could cause validation failures when creating/updating tickets directly.

**Fix**: Updated `ValidateTicket` to accept all valid enum values (0, 1, 2).

---

## Order of Operations in BuyTicketAsync

**Status**: ⚠️ Documented (not critical for Lab 2)

**Issue**: In `BuyTicketAsync`, bonus operations (credit/debit balance) are performed **before** `_ticketGateway.CreateAsync`. If ticket creation fails after bonus operations, there's no compensation mechanism.

**Current Flow**:
1. Create privilege if user doesn't exist
2. Debit/Credit bonus balance
3. Create ticket via gateway

**Risk**: If step 3 fails after step 2, bonus balance is modified but ticket is not created.

**Recommendation**: This is a SAGA pattern problem — requires distributed transaction handling. Out of scope for Lab 2. Will be addressed in Lab 3 (Reliability/Fault Tolerance).

---

## TODO

- [ ] Add seed data step to CI autograding job
- [ ] Verify newman tests pass locally before committing
- [ ] Update gateway Postman collection if status code mismatch with instructor's collection

---

## 2025-01-XX: Instructor Postman Tests Fixed

### Issue
Instructor Postman collection failed with 1 assertion failure:
- `expected undefined not to be undefined` in "Информация о пользователе"
- Root cause: `response.privilege` = undefined

### Root Cause
`UserInfoDTO.PrivilegeInfo` property serialized as `privilegeInfo` (camelCase),
but instructor collection expected `privilege`.

### Solution
Added `[JsonPropertyName("privilege")]` attribute to `PrivilegeInfo` property
in `UserInfoDTO.cs` to override JSON serialization key.

### Additional Finding
Instructor collection requires `flightNumber` environment variable to be set
before running tests (used in POST /tickets request body).

### CI Update
Added `--env-var "flightNumber=AFL031"` to autograding job in `.github/workflows/ci.yml`.

### Result
Newman instructor tests: **8/8 assertions passed**.
