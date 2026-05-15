import { api } from "./client";

export interface DirectoryUser {
  id: number;        // employee id — matches TimeLogDto.employeeId, OvertimeRecordDto.employeeId, etc.
  name: string;
  email: string;
}

// Subset of the backend EmployeeDto we actually consume for the directory picker.
interface EmployeeRow {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
}

export const apiUsers = {
  list: async (): Promise<DirectoryUser[]> => {
    const { data } = await api.get<EmployeeRow[]>("/employees");
    return data.map((d) => ({
      id: d.id,
      name: `${d.firstName} ${d.lastName}`,
      email: d.email,
    }));
  },
};
