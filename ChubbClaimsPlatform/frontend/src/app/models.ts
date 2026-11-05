export interface LoginResponse { token: string; name: string; role: string; }
export interface Claim {
  id: string; claimNumber: string; claimType: string; policyNumber: string;
  incidentDate: string; incidentLocation: string; description: string;
  estimatedAmount: number; approvedAmount?: number; settlementAmount?: number;
  market: string; currency: string; status: string; createdAt: string;
}
