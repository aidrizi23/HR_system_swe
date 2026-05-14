import { api } from "./client";

export interface DirectoryUser {
  id: number;        // employee id — matches TimeLogDto.employeeId, OvertimeRecordDto.employeeId, etc.
  name: string;
  email: string;
}

interface EmployeeDirectoryDto {
  id: number;
  publicId: string;
  firstName: string;
  lastName: string;
  email: string;
  jobTitle?: string;
  departmentName?: string;
  profilePhotoUrl?: string;
  slug: string;
}

export const apiUsers = {
  list: async (): Promise<DirectoryUser[]> => {
    const { data } = await api.get<EmployeeDirectoryDto[]>("/employees/directory");
    return data.map((d) => ({
      id: d.id,
      name: `${d.firstName} ${d.lastName}`,
      email: d.email,
    }));
  },
};
